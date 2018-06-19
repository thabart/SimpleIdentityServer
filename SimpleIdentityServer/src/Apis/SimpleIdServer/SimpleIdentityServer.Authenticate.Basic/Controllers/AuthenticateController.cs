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
using SimpleBus.Core;
using SimpleIdentityServer.Authenticate.Basic.Extensions;
using SimpleIdentityServer.Authenticate.Basic.ViewModels;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt.Extensions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Handler.Events;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Controllers.Website;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.Basic.Controllers
{
    [Area("Authentication")]
    public class AuthenticateController : BaseController
    {
        private const string ExternalAuthenticateCookieName = "SimpleIdentityServer-{0}";        
        private const string DefaultLanguage = "en";        
        private readonly IAuthenticateHelper _authenticateHelper;
        private readonly IAuthenticateActions _authenticateActions;
        private readonly IProfileActions _profileActions;
        private readonly IDataProtector _dataProtector;
        private readonly IEncoder _encoder;
        private readonly ITranslationManager _translationManager;        
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;        
        private readonly IUrlHelper _urlHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IUserActions _userActions;
        private readonly IPayloadSerializer _payloadSerializer;
        private readonly IConfigurationService _configurationService;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
        private readonly BasicAuthenticateOptions _basicAuthenticateOptions;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IProfileActions profileActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUserActions userActions,
            IPayloadSerializer payloadSerializer,
            IConfigurationService configurationService,
            IAuthenticateHelper authenticateHelper,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            BasicAuthenticateOptions basicAuthenticateOptions,
            AuthenticateOptions authenticateOptions) : base(authenticationService, authenticateOptions)
        {
            _authenticateActions = authenticateActions;
            _profileActions = profileActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _encoder = encoder;
            _translationManager = translationManager;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;            
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _userActions = userActions;
            _configurationService = configurationService;
            _authenticateHelper = authenticateHelper;
            _basicAuthenticateOptions = basicAuthenticateOptions;
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
            Check();
        }
        
        #region Public methods
        
        public async Task<ActionResult> Logout()
        {
            HttpContext.Response.Cookies.Delete(SimpleIdentityServer.Core.Constants.SESSION_ID);
            await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.CookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
            return RedirectToAction("Index", "Authenticate");
        }
                
        #region Normal authentication process
        
        public async Task<ActionResult> Index(string name)
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser == null ||
                authenticatedUser.Identity == null ||
                !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }
        
        [HttpPost]
        public async Task<ActionResult> LocalLogin(AuthorizeViewModel authorizeViewModel)
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser != null &&
                authenticatedUser.Identity != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
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
                if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
                {
                    await SetLocalCookie(claims, Guid.NewGuid().ToString());
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                    return RedirectToAction("Index", "User", new { area = "UserManagement" });
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

            // 1. Get the authenticated user.
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName);
            var resourceOwner = await _profileActions.GetResourceOwner(authenticatedUser.GetSubject());

            // 2. Automatically create the resource owner.
            if (resourceOwner == null)
            {
                await AddExternalUser(authenticatedUser);
            }

            IEnumerable<Claim> claims = authenticatedUser.Claims;
            if (resourceOwner != null)
            {
                claims = resourceOwner.Claims;
            }

            await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties());

            // 3. Two factor authentication.
            if (resourceOwner != null && !string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
            {
                await SetTwoFactorCookie(claims);
                try
                {
                    var code = await _authenticateActions.GenerateAndSendCode(resourceOwner.Id);
                    _simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode");
                }
                catch(ClaimRequiredException)
                {
                    return RedirectToAction("SendCode");
                }
            }

            // 4. Set cookie
            await SetLocalCookie(claims.ToOpenidClaims(), Guid.NewGuid().ToString());
            await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties());

            // 5. Redirect to the profile
            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpGet]
        public async Task<ActionResult> SendCode(string code)
        {
            // 1. Retrieve user
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.TwoFactorCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Return translated view.
            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            var service = _twoFactorAuthenticationHandler.Get(resourceOwner.TwoFactorAuthentication);
            var viewModel = new CodeViewModel
            {
                AuthRequestCode = code,
                ClaimName = service.RequiredClaim
            };
            var claim = resourceOwner.Claims.FirstOrDefault(c => c.Type == service.RequiredClaim);
            if (claim != null)
            {
                viewModel.ClaimValue = claim.Value;
            }

            ViewBag.IsAuthenticated = false;
            await TranslateView(DefaultLanguage);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(CodeViewModel codeViewModel)
        {
            if (codeViewModel == null)
            {
                throw new ArgumentNullException(nameof(codeViewModel));
            }

            ViewBag.IsAuthenticated = false;
            codeViewModel.Validate(ModelState);
            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage);
                return View(codeViewModel);
            }

            // 1. Check user is authenticated
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, Host.Constants.TwoFactorCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // 2. Resend the confirmation code.
            if (codeViewModel.Action == CodeViewModel.RESEND_ACTION)
            {
                await TranslateView(DefaultLanguage);
                var resourceOwner = await _userActions.GetUser(authenticatedUser);
                var claim = resourceOwner.Claims.FirstOrDefault(c => c.Type == codeViewModel.ClaimName);
                if (claim != null)
                {
                    resourceOwner.Claims.Remove(claim);
                }

                resourceOwner.Claims.Add(new Claim(codeViewModel.ClaimName, codeViewModel.ClaimValue));
                var claimsLst = resourceOwner.Claims.Select(c => new ClaimAggregate(c.Type, c.Value));
                await _userActions.UpdateClaims(authenticatedUser.GetSubject(), claimsLst);
                var code = await _authenticateActions.GenerateAndSendCode(authenticatedUser.GetSubject());
                _simpleIdentityServerEventSource.GetConfirmationCode(code);
                return View(codeViewModel);
            }

            // 3. Validate the confirmation code
            if (!await _authenticateActions.ValidateCode(codeViewModel.Code))
            {
                await TranslateView(DefaultLanguage);
                ModelState.AddModelError("Code", "confirmation code is not valid");
                _simpleIdentityServerEventSource.ConfirmationCodeNotValid(codeViewModel.Code);
                return View(codeViewModel);
            }

            // 4. Remove the code
            if (!await _authenticateActions.RemoveCode(codeViewModel.Code))
            {
                await TranslateView(DefaultLanguage);
                ModelState.AddModelError("Code", "an error occured while trying to remove the code");
                return View(codeViewModel);
            }

            // 5. Authenticate the resource owner
            await _authenticationService.SignOutAsync(HttpContext, Host.Constants.TwoFactorCookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());

            // 6. Redirect the user agent
            if (!string.IsNullOrWhiteSpace(codeViewModel.AuthRequestCode))
            {
                var request = _dataProtector.Unprotect<AuthorizationRequest>(codeViewModel.AuthRequestCode);
                await SetLocalCookie(authenticatedUser.Claims, request.SessionId);
                var actionResult = await _authenticateHelper.ProcessRedirection(request.ToParameter(), codeViewModel.AuthRequestCode, authenticatedUser.GetSubject(), authenticatedUser.Claims.ToList());
                LogAuthenticateUser(actionResult, request.ProcessId);
                var result = this.CreateRedirectionFromActionResult(actionResult, request);
                return result;
            }
            else
            {
                await SetLocalCookie(authenticatedUser.Claims, Guid.NewGuid().ToString());
            }

            // 7. Redirect the user agent to the User view.
            return RedirectToAction("Index", "User", new { area = "UserManagement" });
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
                authenticatedUser,
                code);
            var result = this.CreateRedirectionFromActionResult(actionResult,
                request);
            if (result != null)
            {
                LogAuthenticateUser(actionResult, request.ProcessId);
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

                // 5. Two factor authentication.
                if (!string.IsNullOrWhiteSpace(actionResult.TwoFactor))
                {
                    await SetTwoFactorCookie(actionResult.Claims);
                    var code = await _authenticateActions.GenerateAndSendCode(subject);
                    _simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode", new { code = authorizeOpenId.Code });
                }
                
                // 6. Authenticate the user by adding a cookie
                await SetLocalCookie(actionResult.Claims, request.SessionId);
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                // 7. Redirect the user agent
                var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult,
                    request);
                if (result != null)
                {
                    LogAuthenticateUser(actionResult.ActionResult, request.ProcessId);
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
            await _authenticationService.ChallengeAsync(HttpContext, provider, new AuthenticationProperties
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
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, string.Format(Core.Errors.ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }            
            
            // 4. Check if the user is authenticated
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName);
            if (authenticatedUser == null ||
                !authenticatedUser.Identity.IsAuthenticated ||
                !(authenticatedUser.Identity is ClaimsIdentity)) {
                  throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, Core.Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }
            
            // 5. Rerieve the claims & insert the resource owner if needed.
            var claimsIdentity = authenticatedUser.Identity as ClaimsIdentity;
            var claims = authenticatedUser.Claims.ToList();
            var resourceOwner = await _profileActions.GetResourceOwner(authenticatedUser.GetSubject());
            if (resourceOwner == null)
            {
                await AddExternalUser(authenticatedUser);
            }

            if (resourceOwner != null)
            {
                claims = resourceOwner.Claims.ToList();
            }

            var subject = claims.GetSubject();

            if (resourceOwner != null && !string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
            {
                await SetTwoFactorCookie(claims);
                var confirmationCode = await _authenticateActions.GenerateAndSendCode(resourceOwner.Id);
                _simpleIdentityServerEventSource.GetConfirmationCode(confirmationCode);
                return RedirectToAction("SendCode", new { code = request });
            }

            // 6. Try to authenticate the resource owner & returns the claims.
            var authorizationRequest = _dataProtector.Unprotect<AuthorizationRequest>(request);
            var actionResult = await _authenticateHelper.ProcessRedirection(authorizationRequest.ToParameter(), request, subject, claims);

            // 7. Store claims into new cookie
            if (actionResult != null)
            {
                await SetLocalCookie(claims.ToOpenidClaims(), authorizationRequest.SessionId);
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new Microsoft.AspNetCore.Authentication.AuthenticationProperties());
                LogAuthenticateUser(actionResult, authorizationRequest.ProcessId);
                return this.CreateRedirectionFromActionResult(actionResult, authorizationRequest);
            }

            return RedirectToAction("OpenId", "Authenticate", new { code = code });
        }

        #endregion

        #endregion

        #region Private methods

        /// <summary>
        /// Add an external account.
        /// </summary>
        /// <param name="authenticatedUser"></param>
        /// <returns></returns>
        private async Task AddExternalUser(ClaimsPrincipal authenticatedUser)
        {
            var openidClaims = authenticatedUser.Claims.ToOpenidClaims().ToList();
            if (_basicAuthenticateOptions.ClaimsIncludedInUserCreation != null && _basicAuthenticateOptions.ClaimsIncludedInUserCreation.Any())
            {
                var lstIndexesToRemove = openidClaims.Where(oc => !_basicAuthenticateOptions.ClaimsIncludedInUserCreation.Contains(oc.Type))
                    .Select(oc => openidClaims.IndexOf(oc))
                    .OrderByDescending(oc => oc);
                foreach(var index in lstIndexesToRemove)
                {
                    openidClaims.RemoveAt(index);
                }
            }

            var record = new AddUserParameter(authenticatedUser.GetSubject(), Guid.NewGuid().ToString(), openidClaims);
            if (_basicAuthenticateOptions.IsScimResourceAutomaticallyCreated)
            {
                await _userActions.AddUser(record, new AuthenticationParameter
                {
                    ClientId = _basicAuthenticateOptions.AuthenticationOptions.ClientId,
                    ClientSecret = _basicAuthenticateOptions.AuthenticationOptions.ClientSecret,
                    WellKnownAuthorizationUrl = _basicAuthenticateOptions.AuthenticationOptions.AuthorizationWellKnownConfiguration
                }, _basicAuthenticateOptions.ScimBaseUrl, true, authenticatedUser.Identity.AuthenticationType);
            }
            else
            {
                await _userActions.AddUser(record, null, null, false, authenticatedUser.Identity.AuthenticationType);
            }
        }

        private void Check()
        {
            if (_basicAuthenticateOptions.IsScimResourceAutomaticallyCreated && (_basicAuthenticateOptions.AuthenticationOptions == null ||
                string.IsNullOrWhiteSpace(_basicAuthenticateOptions.AuthenticationOptions.AuthorizationWellKnownConfiguration) ||
                string.IsNullOrWhiteSpace(_basicAuthenticateOptions.AuthenticationOptions.ClientId) ||
                string.IsNullOrWhiteSpace(_basicAuthenticateOptions.AuthenticationOptions.ClientSecret) ||
                string.IsNullOrWhiteSpace(_basicAuthenticateOptions.ScimBaseUrl)))
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, Core.Errors.ErrorDescriptions.TheScimConfigurationMustBeSpecified);
            }
        }

        private void LogAuthenticateUser(Core.Results.ActionResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ResourceOwnerAuthenticated(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(act), 2));
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
                Core.Constants.StandardTranslationCodes.SendConfirmationCode,
                Core.Constants.StandardTranslationCodes.UpdateClaim,
                Core.Constants.StandardTranslationCodes.ConfirmationCode,
                Core.Constants.StandardTranslationCodes.ResetConfirmationCode,
                Core.Constants.StandardTranslationCodes.ValidateConfirmationCode
            });

            ViewBag.Translations = translations;
        }

        private async Task SetLocalCookie(IEnumerable<Claim> claims, string sessionId)
        {
            var cls = claims.ToList();
            var tokenValidity = await _configurationService.GetTokenValidityPeriodInSecondsAsync();
            var now = DateTime.UtcNow;
            var expires = now.AddSeconds(tokenValidity);
            HttpContext.Response.Cookies.Append(SimpleIdentityServer.Core.Constants.SESSION_ID, sessionId, new CookieOptions
            {
                HttpOnly = false,
                Expires = expires
            });
            var identity = new ClaimsIdentity(cls, _authenticateOptions.CookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, _authenticateOptions.CookieName, principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IssuedUtc = now,
                ExpiresUtc = expires,
                AllowRefresh = false,
                IsPersistent = false
            });
        }

        private async Task SetTwoFactorCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, SimpleIdentityServer.Host.Constants.TwoFactorCookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, SimpleIdentityServer.Host.Constants.TwoFactorCookieName, principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
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