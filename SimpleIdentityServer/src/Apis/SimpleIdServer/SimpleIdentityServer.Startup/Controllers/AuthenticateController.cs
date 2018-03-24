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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.EventStore.Core.Models;
using SimpleIdentityServer.EventStore.Core.Repositories;
using SimpleIdentityServer.Handler.Bus;
using SimpleIdentityServer.Handler.Events;
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
    public class AuthenticateController : BaseController
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
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IPayloadSerializer _payloadSerializer;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventAggregateRepository eventAggregateRepository,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUserActions userActions,
            IPayloadSerializer payloadSerializer) : base(authenticationService, userActions)
        {
            _authenticateActions = authenticateActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _encoder = encoder;
            _translationManager = translationManager;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;            
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _eventAggregateRepository = eventAggregateRepository;
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }
        
        #region Public methods
        
        public async Task<ActionResult> Logout()
        {
            await _authenticationService.SignOutAsync(HttpContext, Constants.CookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
            return RedirectToAction("Index", "Authenticate");
        }
                
        #region Normal authentication process
        
        public async Task<ActionResult> Index(string name)
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser.Key == null ||
                authenticatedUser.Key.Identity == null ||
                !authenticatedUser.Key.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User");
        }
        
        [HttpPost]
        public async Task<ActionResult> LocalLogin(AuthorizeViewModel authorizeViewModel)
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser.Key != null &&
                authenticatedUser.Key.Identity != null &&
                authenticatedUser.Key.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException(nameof(authorizeViewModel));
            }

            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage);
                await SetIdProviders(authorizeViewModel);
                return View("Index", authorizeViewModel);
            }

            try
            {
                var resourceOwner = await _authenticateActions.LocalUserAuthentication(authorizeViewModel.ToParameter());
                var claims = resourceOwner.Claims;
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
                var subject = claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                if (resourceOwner.TwoFactorAuthentication == Core.Models.TwoFactorAuthentications.NONE)
                {
                    await SetLocalCookie(claims);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                    return RedirectToAction("Index", "User");
                }

                // 2.1 Store temporary information in cookie
                await SetTwoFactorCookie(claims);
                // 2.2. Send confirmation code
                var code = await _authenticateActions.GenerateAndSendCode(subject);
                _simpleIdentityServerEventSource.GetConfirmationCode(code);
                return RedirectToAction("SendCode");
            }
            catch (Exception exception)
            {
                _simpleIdentityServerEventSource.Failure(exception.Message);
                await TranslateView("en");
                ModelState.AddModelError("invalid_credentials", exception.Message);
                await SetIdProviders(authorizeViewModel);
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
            await _authenticationService.ChallengeAsync(HttpContext, provider, new Microsoft.AspNetCore.Authentication.AuthenticationProperties() 
            {
                RedirectUri = redirectUrl
            });
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
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Constants.ExternalCookieName);
            var claims = await _authenticateActions.LoginCallback(authenticatedUser);

            // 2. Set cookie
            await SetLocalCookie(claims);
            await _authenticationService.SignOutAsync(HttpContext, Constants.ExternalCookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());

            // 3. Redirect to the profile
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public async Task<ActionResult> SendCode()
        {
            // 1. Retrieve user
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.TwoFactorCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Return translated view
            await TranslateView(DefaultLanguage);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendCode()
        {
            // 1. Retrieve user
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.TwoFactorCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Send the code
            var subject = authenticatedUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
            var code = await _authenticateActions.GenerateAndSendCode(subject);
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

            await SetUser();
            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage);
                return View(codeViewModel);
            }

            // 1. Check user is authenticated
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.TwoFactorCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Validate the code
            if (!await _authenticateActions.ValidateCode(codeViewModel.Code))
            {
                await TranslateView(DefaultLanguage);
                ModelState.AddModelError("Code", "confirmation code is not valid");
                _simpleIdentityServerEventSource.ConfirmationCodeNotValid(codeViewModel.Code);
                return View(codeViewModel);
            }

            // 3. Remove the code
            if (!await _authenticateActions.RemoveCode(codeViewModel.Code))
            {
                await TranslateView(DefaultLanguage);
                ModelState.AddModelError("Code", "an error occured while trying to remove the code");
                return View(codeViewModel);
            }

            // 4. Authenticate the resource owner
            await _authenticationService.SignOutAsync(HttpContext, Host.Constants.TwoFactorCookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
            await SetLocalCookie(authenticatedUser.Claims);
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

            var authenticatedUser = await SetUser();
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var actionResult = await _authenticateActions.AuthenticateResourceOwnerOpenId(
                request.ToParameter(),
                authenticatedUser.Key,
                code);
            var result = this.CreateRedirectionFromActionResult(actionResult,
                request);
            if (result != null)
            {
                await LogAuthenticateUser(actionResult, request.ProcessId);
                return result;
            }

            await TranslateView(request.UiLocales);
            var viewModel = new AuthorizeOpenIdViewModel
            {
                Code = code
            };

            await SetIdProviders(viewModel);
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

            await SetUser();
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
                    await TranslateView(uiLocales);
                    await SetIdProviders(authorizeOpenId);
                    return View("OpenId", authorizeOpenId);
                }

                // 4. Local authentication
                var actionResult = await _authenticateActions.LocalOpenIdUserAuthentication(authorizeOpenId.ToParameter(),
                    request.ToParameter(),
                    authorizeOpenId.Code);
                var subject = actionResult.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;

                // 5. Authenticate the user by adding a cookie
                await SetLocalCookie(actionResult.Claims);
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                // 6. Redirect the user agent
                var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult,
                    request);
                if (result != null)
                {
                    await LogAuthenticateUser(actionResult.ActionResult, request.ProcessId);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            await TranslateView(uiLocales);
            await SetIdProviders(authorizeOpenId);
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
            await _authenticationService.ChallengeAsync(HttpContext, provider, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = redirectUrl
            });
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
            var authenticatedUser = await this.GetAuthenticatedUserExternal();
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
                request);

            // 7. Store claims into new cookie
            if (actionResult.ActionResult != null)
            {
                await SetLocalCookie(actionResult.Claims);
                await _authenticationService.SignOutAsync(HttpContext, Constants.ExternalCookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
                await LogAuthenticateUser(actionResult.ActionResult, authorizationRequest.ProcessId);
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

            var evtAggregate = await GetLastEventAggregate(processId);
            if (evtAggregate == null)
            {
                return;
            }

            _eventPublisher.Publish(new ResourceOwnerAuthenticated(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(act), evtAggregate.Order + 1));
        }

        private async Task<EventAggregate> GetLastEventAggregate(string aggregateId)
        {
            var events = (await _eventAggregateRepository.GetByAggregate(aggregateId)).OrderByDescending(e => e.Order);
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
            });

            ViewBag.Translations = translations;
        }

        private async Task SetLocalCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Constants.CookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, Constants.CookieName, principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddDays(7),
                IsPersistent = false
            });
        }

        private async Task SetTwoFactorCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Host.Constants.TwoFactorCookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, Host.Constants.TwoFactorCookieName, principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(5),
                IsPersistent = false
            });
        }

        private async Task SetIdProviders(AuthorizeViewModel authorizeViewModel)
        {
            var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync()).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName));
            var idProviders = new List<IdProviderViewModel>();
            foreach (var scheme in schemes)
            {
                idProviders.Add(new IdProviderViewModel
                {
                    AuthenticationScheme = scheme.Name,
                    DisplayName = scheme.DisplayName
                });
            }

            authorizeViewModel.IdProviders = idProviders;
        }

        private async Task SetIdProviders(AuthorizeOpenIdViewModel authorizeViewModel)
        {
            var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync()).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName));
            var idProviders = new List<IdProviderViewModel>();
            foreach (var scheme in schemes)
            {
                idProviders.Add(new IdProviderViewModel
                {
                    AuthenticationScheme = scheme.Name,
                    DisplayName = scheme.DisplayName
                });
            }

            authorizeViewModel.IdProviders = idProviders;
        }

        #endregion
    }
}