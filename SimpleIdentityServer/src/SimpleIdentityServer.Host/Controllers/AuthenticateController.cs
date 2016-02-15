using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Api.ViewModels;
using Microsoft.AspNet.Authentication.Cookies;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.ViewModels;
using Microsoft.AspNet.Http;
using System.Linq;

namespace SimpleIdentityServer.Host.Controllers
{
    public class AuthenticateController : Controller
    {
        private const string ExternalAuthenticateCookieName = "SimpleIdentityServer-{0}";
        
        private const string DefaultLanguage = "en";
        
        private readonly IAuthenticateActions _authenticateActions;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly ITranslationManager _translationManager;
        
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        
        private readonly IUrlHelper _urlHelper;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IProtector protector,
            IEncoder encoder,
            ITranslationManager translationManager,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IUrlHelper urlHelper)
        {
            _authenticateActions = authenticateActions;
            _protector = protector;
            _encoder = encoder;
            _translationManager = translationManager;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;            
            _urlHelper = urlHelper;
        }
        
        #region Public methods
        
        public ActionResult Logout()
        {
            var authenticationManager = this.GetAuthenticationManager();
            authenticationManager.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Index", "Authenticate");
        }
                
        #region Normal authentication process
        
        public ActionResult Index()
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            if (authenticatedUser == null ||
                !authenticatedUser.Identity.IsAuthenticated)
            {
                TranslateView(DefaultLanguage);
                return View(new AuthorizeViewModel());
            }

            return RedirectToAction("Index", "User");
        }
        
        [HttpPost]
        public ActionResult LocalLogin(AuthorizeViewModel authorizeViewModel)
        {
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
                var claims = _authenticateActions.LocalUserAuthentication(authorizeViewModel.ToParameter());
                var authenticationManager = this.GetAuthenticationManager();
                var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var principal = new ClaimsPrincipal(claimsIdentity);
                authenticationManager.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties {
                        ExpiresUtc = DateTime.UtcNow.AddDays(7),
                        IsPersistent = false
                    });

                return RedirectToAction("Index", "User");
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
            await this.HttpContext.Authentication.ChallengeAsync(provider, new AuthenticationProperties() 
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

            // 1. Check if the user is authenticated
            var authenticatedUser = this.GetAuthenticatedUser();
            if (authenticatedUser == null ||
                !authenticatedUser.Identity.IsAuthenticated ||
                !(authenticatedUser.Identity is ClaimsIdentity)) {
                  throw new IdentityServerException(
                        Core.Errors.ErrorCodes.UnhandledExceptionCode,
                        Core.Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }
            
            // 2. Redirect to the profile
            return RedirectToAction("Index", "User");
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
            var decodedCode = _encoder.Decode(code);
            var request = _protector.Decrypt<AuthorizationRequest>(decodedCode);

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
                var code = _encoder.Decode(authorizeOpenId.Code);
                var request = _protector.Decrypt<AuthorizationRequest>(code);
                
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
                var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                authenticationManager.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    }
                ).Wait();

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

            // 1. Persist the request code into a cookie
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
            var request = Request.Cookies[string.Format(ExternalAuthenticateCookieName, code)].FirstOrDefault();
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
            var decodedRequest = _encoder.Decode(request);
            var authorizationRequest = _protector.Decrypt<AuthorizationRequest>(decodedRequest);
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
                Core.Constants.StandardTranslationCodes.LoginExternalAccount
            });

            ViewBag.Translations = translations;
        }
        
        #endregion
    }
}