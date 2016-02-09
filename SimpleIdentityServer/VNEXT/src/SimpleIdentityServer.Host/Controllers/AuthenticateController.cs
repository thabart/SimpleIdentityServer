using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
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

namespace SimpleIdentityServer.Host.Controllers
{
    public class AuthenticateController : Controller
    {
        private const string AuthenticationScheme = "SimpleIdentityServerAuthentication";
        
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
            authenticationManager.SignOutAsync(AuthenticationScheme).Wait();
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
                authenticationManager.SignInAsync(AuthenticationScheme,
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
        
        [HttpPost(Name = "ExternalLogin")]
        public async Task ExternalLogin(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            string url = _urlHelper.RouteUrl("LoginCallback", null);
            await this.HttpContext.Authentication.ChallengeAsync(provider, new AuthenticationProperties() 
            {
                RedirectUri = "http://localhost:5000/Authenticate/LoginCallback"
            });
            return;
        }
        
        [HttpGet(Name = "LoginCallback")]
        public ActionResult LoginCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(
                    Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    string.Format(Core.Errors.ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            var authenticationManager = this.GetAuthenticationManager();
            authenticationManager.GetAuthenticationSchemes();
            var u = this.GetAuthenticatedUser(); 
            
            // Put a break point to see authenticated user
            // var loginInformation = null;
            // ExternalLoginInfo loginInformation = await authenticationManager.
            /*
            if (loginInformation == null)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Core.Errors.ErrorDescriptions.TheLoginInformationCannotBeExtracted);
            }
            
            var providerType = loginInformation.Login.LoginProvider;
            var claims = loginInformation.ExternalIdentity.Claims.ToList();
            var openIdClaims = _authenticateActions.ExternalUserAuthentication(claims, providerType);
            var claimsIdentity = new ClaimsIdentity(openIdClaims, DefaultAuthenticationTypes.ApplicationCookie);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            authenticationManager.SignInAsync(AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                }
            ).Wait();
            */
            return RedirectToAction("Index", "User");
        }

        #endregion

        #endregion

        /*
        [HttpGet]
        public ActionResult Index(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) {
                throw new ArgumentNullException("code");
            }
            
            var authenticatedUser = this.GetAuthenticatedUser();
            var decodedCode = _encoder.Decode(code);
            var request = _protector.Decrypt<AuthorizationRequest>(decodedCode);

            var actionResult = _authenticateActions.AuthenticateResourceOwner(
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
            var viewModel = new AuthorizeViewModel
            {
                Code = code
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(AuthorizeViewModel authorize)
        {
            var code = _encoder.Decode(authorize.Code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            try
            {
                var claims = new List<Claim>();
                var actionResult = _authenticateActions.LocalUserAuthentication(authorize.ToParameter(),
                    request.ToParameter(),
                    authorize.Code,
                    out claims);


                var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var principal = new ClaimsPrincipal(claimsIdentity);
                HttpContext.Authentication.SignInAsync("SimpleIdentityServerAuthentication", 
                    principal,
                    new AuthenticationProperties {
                        ExpiresUtc = DateTime.UtcNow.AddDays(7),
                        IsPersistent = false
                    }).Wait();

                var result = this.CreateRedirectionFromActionResult(actionResult,
                    request);
                if (result != null)
                {
                    return result;
                }

            }
            catch (IdentityServerAuthenticationException)
            {
                // TODO : log the exception
            }

            TranslateView(request.ui_locales);
            return View(authorize);
        }
        
        #endregion
        */

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
    }
}