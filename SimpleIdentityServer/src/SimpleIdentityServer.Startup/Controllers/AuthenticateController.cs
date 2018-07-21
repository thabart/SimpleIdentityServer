#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Events;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Startup.Extensions;
using SimpleIdentityServer.Startup.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Startup.Controllers
{
    public class AuthenticateController : Controller
    {
        private const string ExternalAuthenticateCookieName = "SimpleIdentityServer-{0}";        
        private const string DefaultLanguage = "en";        
        private readonly IAuthenticateActions _authenticateActions;
        private readonly IDataProtector _dataProtector;
        private readonly IEncoder _encoder;
        private readonly ITranslationManager _translationManager;        
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;        
        private readonly IUrlHelper _urlHelper;
        private readonly IEventAggregateRepository _eventAggregateRepository;
        private readonly IEventPublisher _eventPublisher;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventAggregateRepository eventAggregateRepository,
            IEventPublisher eventPublisher)
        {
            _authenticateActions = authenticateActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _encoder = encoder;
            _translationManager = translationManager;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;            
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _eventAggregateRepository = eventAggregateRepository;
            _eventPublisher = eventPublisher;
        }
        
        #region Public methods
        
        public async Task<ActionResult> Logout()
        {
            var authenticationManager = this.GetAuthenticationManager();
            await authenticationManager.SignOutAsync(Constants.CookieName).ConfigureAwait(false);
            return RedirectToAction("Index", "Authenticate");
        }
                
        #region Normal authentication process
        
        public async Task<ActionResult> Index(string name)
        {
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            if (authenticatedUser == null ||
                authenticatedUser.Identity == null ||
                !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View(new AuthorizeViewModel());
            }

            return RedirectToAction("Index", "User");
        }
        
        [HttpPost]
        public async Task<ActionResult> LocalLogin(AuthorizeViewModel authorizeViewModel)
        {
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            if (authenticatedUser != null &&
                authenticatedUser.Identity != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException(nameof(authorizeViewModel));
            }

            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View("Index", authorizeViewModel);
            }

            try
            {
                var resourceOwner = await _authenticateActions.LocalUserAuthentication(authorizeViewModel.ToParameter()).ConfigureAwait(false);
                var claims = resourceOwner.Claims;
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
                var subject = claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                var authenticationManager = this.GetAuthenticationManager();
                if (resourceOwner.TwoFactorAuthentication == Core.Models.TwoFactorAuthentications.NONE)
                {
                    await SetLocalCookie(authenticationManager, claims).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                    return RedirectToAction("Index", "User");
                }

                // 2.1 Store temporary information in cookie
                await SetTwoFactorCookie(authenticationManager, claims).ConfigureAwait(false);
                // 2.2. Send confirmation code
                var code = await _authenticateActions.GenerateAndSendCode(subject).ConfigureAwait(false);
                _simpleIdentityServerEventSource.GetConfirmationCode(code);
                return RedirectToAction("SendCode");
            }
            catch (Exception exception)
            {
                _simpleIdentityServerEventSource.Failure(exception.Message);
                await TranslateView("en").ConfigureAwait(false);
                ModelState.AddModelError("invalid_credentials", exception.Message);
                return View("Index", authorizeViewModel);
            }
        }
        
        [HttpPost]
        public async Task ExternalLogin(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var redirectUrl = _urlHelper.AbsoluteAction("LoginCallback", "Authenticate");
            await HttpContext.Authentication.ChallengeAsync(provider, new AuthenticationProperties() 
            {
                RedirectUri = redirectUrl
            }).ConfigureAwait(false);
        }
        
        [HttpGet]
        public async Task<ActionResult> LoginCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    string.Format(Core.Errors.ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            // 1. Check if the user exists and insert it
            var authenticatedUser = await this.GetAuthenticatedUserExternal().ConfigureAwait(false);
            var claims = await _authenticateActions.LoginCallback(authenticatedUser).ConfigureAwait(false);

            // 2. Set cookie
            var authManager = this.GetAuthenticationManager();
            await SetLocalCookie(authManager, claims).ConfigureAwait(false);
            await authManager.SignOutAsync(Authentication.Middleware.Constants.CookieName).ConfigureAwait(false);

            // 3. Redirect to the profile
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public async Task<ActionResult> SendCode()
        {
            // 1. Retrieve user
            var authenticatedUser = await this.GetAuthenticatedUser2F().ConfigureAwait(false);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Return translated view
            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendCode()
        {
            // 1. Retrieve user
            var authenticatedUser = await this.GetAuthenticatedUser2F().ConfigureAwait(false);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Send the code
            var subject = authenticatedUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
            var code = await _authenticateActions.GenerateAndSendCode(subject).ConfigureAwait(false);
            _simpleIdentityServerEventSource.GetConfirmationCode(code);

            // 3. Redirect to code view
            return RedirectToAction("SendCode");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(CodeViewModel codeViewModel)
        {
            if (codeViewModel == null)
            {
                throw new ArgumentNullException(nameof(codeViewModel));
            }

            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View(codeViewModel);
            }

            // 1. Check user is authenticated
            var authenticatedUser = await this.GetAuthenticatedUser2F().ConfigureAwait(false);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Validate the code
            if (!await _authenticateActions.ValidateCode(codeViewModel.Code).ConfigureAwait(false))
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                ModelState.AddModelError("Code", "confirmation code is not valid");
                _simpleIdentityServerEventSource.ConfirmationCodeNotValid(codeViewModel.Code);
                return View(codeViewModel);
            }

            // 3. Remove the code
            if (!await _authenticateActions.RemoveCode(codeViewModel.Code).ConfigureAwait(false))
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                ModelState.AddModelError("Code", "an error occured while trying to remove the code");
                return View(codeViewModel);
            }

            // 4. Authenticate the resource owner
            var authenticationManager = this.GetAuthenticationManager();
            await authenticationManager.SignOutAsync(Host.Constants.TwoFactorCookieName).ConfigureAwait(false);
            await SetLocalCookie(authenticationManager, authenticatedUser.Claims).ConfigureAwait(false);
            return RedirectToAction("Index", "User");
        }
        
        #endregion
        
        #region Authentication process which follows OPENID
        
        [HttpGet]
        public async Task<ActionResult> OpenId(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var actionResult = await _authenticateActions.AuthenticateResourceOwnerOpenId(
                request.ToParameter(),
                authenticatedUser,
                code).ConfigureAwait(false);
            var result = this.CreateRedirectionFromActionResult(actionResult,
                request);
            if (result != null)
            {
                await LogAuthenticateUser(actionResult, request.ProcessId).ConfigureAwait(false);
                return result;
            }

            await TranslateView(request.UiLocales).ConfigureAwait(false);
            var viewModel = new AuthorizeOpenIdViewModel
            {
                Code = code
            };

            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<ActionResult> LocalLoginOpenId(AuthorizeOpenIdViewModel authorizeOpenId)
        {
            if (authorizeOpenId == null)
            {
                throw new ArgumentNullException(nameof(authorizeOpenId));
            }

            if (string.IsNullOrWhiteSpace(authorizeOpenId.Code))
            {
                throw new ArgumentNullException(nameof(authorizeOpenId.Code));
            }

            var uiLocales = DefaultLanguage;
            try
            {
                // 1. Decrypt the request
                var request = _dataProtector.Unprotect<AuthorizationRequest>(authorizeOpenId.Code);
                
                // 2. Retrieve the default language
                uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;
                
                // 3. Check the state of the view model
                if (!ModelState.IsValid)
                {
                    await TranslateView(uiLocales).ConfigureAwait(false);
                    return View("OpenId", authorizeOpenId);
                }

                // 4. Local authentication
                var actionResult = await _authenticateActions.LocalOpenIdUserAuthentication(authorizeOpenId.ToParameter(),
                    request.ToParameter(),
                    authorizeOpenId.Code).ConfigureAwait(false);
                var subject = actionResult.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;

                // 5. Authenticate the user by adding a cookie
                var authenticationManager = this.GetAuthenticationManager();
                await SetLocalCookie(authenticationManager, actionResult.Claims).ConfigureAwait(false);
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                // 6. Redirect the user agent
                var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult,
                    request);
                if (result != null)
                {
                    await LogAuthenticateUser(actionResult.ActionResult, request.ProcessId).ConfigureAwait(false);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            await TranslateView(uiLocales).ConfigureAwait(false);
            return View("OpenId", authorizeOpenId);
        }
        
        [HttpPost]
        public async Task ExternalLoginOpenId(string provider, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            // 1. Persist the request code into a cookie & fix the space problems
            var cookieValue = Guid.NewGuid().ToString();
            var cookieName = string.Format(ExternalAuthenticateCookieName, cookieValue);
            Response.Cookies.Append(cookieName, code, 
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(5)
                });

            // 2. Redirect the User agent
            var redirectUrl = _urlHelper.AbsoluteAction("LoginCallbackOpenId", "Authenticate", new { code = cookieValue });
            await this.HttpContext.Authentication.ChallengeAsync(provider, new AuthenticationProperties() 
            {
                RedirectUri = redirectUrl
            }).ConfigureAwait(false);
        }
        
        [HttpGet]
        public async Task<ActionResult> LoginCallbackOpenId(string code, string error)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            // 1 : retrieve the request from the cookie
            var cookieName = string.Format(ExternalAuthenticateCookieName, code);
            var request = Request.Cookies[string.Format(ExternalAuthenticateCookieName, code)];
            if (request == null)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TheRequestCannotBeExtractedFromTheCookie);
            }

            // 2 : remove the cookie
            Response.Cookies.Append(cookieName, string.Empty,
                new CookieOptions{
                    Expires = DateTime.UtcNow.AddDays(-1)
                });

            // 3 : Raise an exception is there's an authentication error
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode, 
                    string.Format(Core.Errors.ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }            
            
            // 4. Check if the user is authenticated
            var authenticatedUser = await this.GetAuthenticatedUserExternal().ConfigureAwait(false);
            if (authenticatedUser == null ||
                !authenticatedUser.Identity.IsAuthenticated ||
                !(authenticatedUser.Identity is ClaimsIdentity)) {
                  throw new IdentityServerException(
                        Core.Errors.ErrorCodes.UnhandledExceptionCode,
                        Core.Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }
            
            // 5. Rerieve the claims
            var claimsIdentity = authenticatedUser.Identity as ClaimsIdentity;
            var claims = claimsIdentity.Claims.ToList();

            // 6. Try to authenticate the resource owner & returns the claims.
            var authorizationRequest = _dataProtector.Unprotect<AuthorizationRequest>(request);
            var actionResult = await _authenticateActions.ExternalOpenIdUserAuthentication(
                claims,
                authorizationRequest.ToParameter(),
                request).ConfigureAwait(false);

            // 7. Store claims into new cookie
            if (actionResult.ActionResult != null)
            {
                var authenticationManager = this.GetAuthenticationManager();
                await SetLocalCookie(authenticationManager, actionResult.Claims).ConfigureAwait(false);
                await authenticationManager.SignOutAsync(Authentication.Middleware.Constants.CookieName).ConfigureAwait(false);
                await LogAuthenticateUser(actionResult.ActionResult, authorizationRequest.ProcessId).ConfigureAwait(false);
                return this.CreateRedirectionFromActionResult(actionResult.ActionResult,
                    authorizationRequest);
            }

            return RedirectToAction("OpenId", "Authenticate", new { code = code });
        }

        #endregion

        #endregion

        #region Private methods

        private async Task LogAuthenticateUser(Core.Results.ActionResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            var evtAggregate = await GetLastEventAggregate(processId).ConfigureAwait(false);
            if (evtAggregate == null)
            {
                return;
            }

            _eventPublisher.Publish(new ResourceOwnerAuthenticated(Guid.NewGuid().ToString(), processId, act, evtAggregate.Order + 1));
        }

        private async Task<EventAggregate> GetLastEventAggregate(string aggregateId)
        {
            var events = (await _eventAggregateRepository.GetByAggregate(aggregateId).ConfigureAwait(false)).OrderByDescending(e => e.Order);
            if (events == null || !events.Any())
            {
                return null;
            }

            return events.First();
        }

        private async Task TranslateView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.LoginCode,
                Core.Constants.StandardTranslationCodes.UserNameCode,
                Core.Constants.StandardTranslationCodes.PasswordCode,
                Core.Constants.StandardTranslationCodes.RememberMyLoginCode,
                Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                Core.Constants.StandardTranslationCodes.SendCode,
                Core.Constants.StandardTranslationCodes.Code,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.SendConfirmationCode
            }).ConfigureAwait(false);

            ViewBag.Translations = translations;
        }

        private async Task SetLocalCookie(AuthenticationManager authenticationManager, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Constants.CookieName);
            var principal = new ClaimsPrincipal(identity);
            await authenticationManager.SignInAsync(Constants.CookieName,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    IsPersistent = false
                }).ConfigureAwait(false);
        }

        private async Task SetTwoFactorCookie(AuthenticationManager authenticationManager, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Host.Constants.TwoFactorCookieName);
            var principal = new ClaimsPrincipal(identity);
            await authenticationManager.SignInAsync(Host.Constants.TwoFactorCookieName,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(5),
                    IsPersistent = false
                }).ConfigureAwait(false);
        }

        #endregion
    }
}