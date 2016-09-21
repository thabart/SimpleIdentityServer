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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers
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

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _authenticateActions = authenticateActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _encoder = encoder;
            _translationManager = translationManager;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;            
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }
        
        #region Public methods
        
        public ActionResult Logout()
        {
            var authenticationManager = this.GetAuthenticationManager();
            authenticationManager.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Index", "Authenticate");
        }
                
        #region Normal authentication process
        
        public ActionResult Index(string name)
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            if (authenticatedUser == null ||
                authenticatedUser.Identity == null ||
                !authenticatedUser.Identity.IsAuthorized())
            {
                TranslateView(DefaultLanguage);
                return View(new AuthorizeViewModel());
            }

            return RedirectToAction("Index", "User");
        }
        
        [HttpPost]
        public ActionResult LocalLogin(AuthorizeViewModel authorizeViewModel)
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            if (authenticatedUser != null &&
                authenticatedUser.Identity != null &&
                authenticatedUser.Identity.IsAuthorized())
            {
                return RedirectToAction("Index", "User");
            }

            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException(nameof(authorizeViewModel));
            }

            if (!ModelState.IsValid)
            {
                TranslateView(DefaultLanguage);
                return View("Index", authorizeViewModel);
            }

            try
            {
                var resourceOwner = _authenticateActions.LocalUserAuthentication(authorizeViewModel.ToParameter());
                var claims = resourceOwner.ToClaims();
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
                var authenticationManager = this.GetAuthenticationManager();
                ClaimsIdentity identity = null;
                ClaimsPrincipal principal = null;
                // If there is no two factor authentication then authenticate the user and redirect to User page
                if (resourceOwner.TwoFactorAuthentication == Core.Models.TwoFactorAuthentications.NONE)
                {
                    identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    principal = new ClaimsPrincipal(identity);
                    authenticationManager.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            ExpiresUtc = DateTime.UtcNow.AddDays(7),
                            IsPersistent = false
                        });
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(identity.Name);
                    return RedirectToAction("Index", "User");
                }

                identity = new ClaimsIdentity(claims, Constants.TwoFactorCookieName);
                principal = new ClaimsPrincipal(identity);
                authenticationManager.SignInAsync(Constants.TwoFactorCookieName,
                    principal,
                    new AuthenticationProperties {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(5),
                        IsPersistent = false
                    });
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(identity.Name);
                return RedirectToAction("SendCode");
            }
            catch (Exception exception)
            {
                _simpleIdentityServerEventSource.Failure(exception.Message);
                TranslateView("en");
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
            });
        }
        
        [HttpGet]
        public ActionResult LoginCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    string.Format(Core.Errors.ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            // 1. Check if the user exists and insert it
            var authenticatedUser = this.GetAuthenticatedUser();
            _authenticateActions.LoginCallback(authenticatedUser);
            
            // 2. Redirect to the profile
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public async Task<ActionResult> SendCode()
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            var user = await HttpContext.Authentication.AuthenticateAsync(Constants.TwoFactorCookieName);
            if (user == null || !user.Claims.Any(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject))
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TwoFactorAuthenticationCannotBePerformed);
            }

            // TODO : SEND A VALID CODE
            await _authenticateActions.GenerateAndSendCode(user.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value);
            TranslateView(DefaultLanguage);
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendCode(CodeViewModel codeViewModel)
        {
            var authenticationManager = this.GetAuthenticationManager();
            var user = await authenticationManager.AuthenticateAsync(Constants.TwoFactorCookieName);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (codeViewModel == null)
            {
                throw new ArgumentNullException(nameof(codeViewModel));
            }

            if (!ModelState.IsValid)
            {
                TranslateView(DefaultLanguage);
                return View(codeViewModel);
            }

            // Validate the code
            if(!_authenticateActions.ValidateCode(codeViewModel.Code))
            {
                TranslateView(DefaultLanguage);
                ModelState.AddModelError("Code", "confirmation code is not valid");
                _simpleIdentityServerEventSource.ConfirmationCodeNotValid(codeViewModel.Code);
                return View(codeViewModel);
            }

            // Remove the code
            if (!_authenticateActions.RemoveCode(codeViewModel.Code))
            {
                TranslateView(DefaultLanguage);
                ModelState.AddModelError("Code", "an error occured while trying to remove the code");
                return View(codeViewModel);
            }

            // Authenticate the resource owner
            await authenticationManager.SignOutAsync(Constants.TwoFactorCookieName);
            var identity = new ClaimsIdentity(user.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authenticatedUser = new ClaimsPrincipal(identity);
            await authenticationManager.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                authenticatedUser,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    IsPersistent = false
                });
            return RedirectToAction("Index", "Home");
        }
        
        #endregion
        
        #region Authentication process which follows OPENID
        
        [HttpGet]
        public ActionResult OpenId(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var authenticatedUser = this.GetAuthenticatedUser();
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);

            var actionResult = _authenticateActions.AuthenticateResourceOwnerOpenId(
                request.ToParameter(),
                authenticatedUser,
                code);
            var result = this.CreateRedirectionFromActionResult(actionResult,
                request);
            if (result != null)
            {
                return result;
            }

            TranslateView(request.ui_locales);
            var viewModel = new AuthorizeOpenIdViewModel
            {
                Code = code
            };

            return View(viewModel);
        }
        
        [HttpPost]
        public ActionResult LocalLoginOpenId(AuthorizeOpenIdViewModel authorizeOpenId)
        {
            if (authorizeOpenId == null)
            {
                throw new ArgumentNullException(nameof(authorizeOpenId));
            }

            if (string.IsNullOrWhiteSpace(authorizeOpenId.Code))
            {
                throw new ArgumentNullException("authorizeOpenId.Code");
            }

            var uiLocales = DefaultLanguage;
            try
            {
                // 1. Decrypt the request
                var request = _dataProtector.Unprotect<AuthorizationRequest>(authorizeOpenId.Code);
                
                // 2. Retrieve the default language
                uiLocales = string.IsNullOrWhiteSpace(request.ui_locales) ? DefaultLanguage : request.ui_locales;
                
                // 3. Check the state of the view model
                if (!ModelState.IsValid)
                {
                    TranslateView(uiLocales);
                    return View("OpenId", authorizeOpenId);
                }

                // 4. Local authentication
                var claims = new List<Claim>();
                var actionResult = _authenticateActions.LocalOpenIdUserAuthentication(authorizeOpenId.ToParameter(),
                    request.ToParameter(),
                    authorizeOpenId.Code,
                    out claims);

                // 5. Authenticate the user by adding a cookie
                var authenticationManager = this.GetAuthenticationManager();
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                authenticationManager.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    }
                ).Wait();
                _simpleIdentityServerEventSource.AuthenticateResourceOwner(claimsIdentity.Name);

                // 6. Redirect the user agent
                var result = this.CreateRedirectionFromActionResult(actionResult,
                    request);
                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            TranslateView(uiLocales);
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
            });
        }
        
        [HttpGet]
        public ActionResult LoginCallbackOpenId(string code, string error)
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
            var authenticatedUser = this.GetAuthenticatedUser();
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
            
            // 6. Continue the open-id flow
            var authorizationRequest = _dataProtector.Unprotect<AuthorizationRequest>(request);
            var actionResult = _authenticateActions.ExternalOpenIdUserAuthentication(
                claims,
                authorizationRequest.ToParameter(),
                request); 
            if (actionResult != null)
            {
                return this.CreateRedirectionFromActionResult(actionResult,
                    authorizationRequest);
            }

            return RedirectToAction("OpenId", "Authenticate", new { code = code });
        }
        
        #endregion

        #endregion
        
        #region Private methods

        private void TranslateView(string uiLocales)
        {
            var translations = _translationManager.GetTranslations(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.LoginCode,
                Core.Constants.StandardTranslationCodes.UserNameCode,
                Core.Constants.StandardTranslationCodes.PasswordCode,
                Core.Constants.StandardTranslationCodes.RememberMyLoginCode,
                Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                Core.Constants.StandardTranslationCodes.SendCode,
                Core.Constants.StandardTranslationCodes.Code,
                Core.Constants.StandardTranslationCodes.ConfirmCode
            });

            ViewBag.Translations = translations;
        }
        
        #endregion
    }
}