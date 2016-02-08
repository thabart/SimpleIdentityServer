using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Mvc;

using ActionResult = System.Web.Mvc.ActionResult;

namespace SimpleIdentityServer.Api.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IAuthenticateActions _authenticateActions;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly ITranslationManager _translationManager;

        private const string ExternalAuthenticateCookieName = "SimpleIdentityServer-{0}";

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IProtector protector,
            IEncoder encoder,
            ITranslationManager translationManager)
        {
            _authenticateActions = authenticateActions;
            _protector = protector;
            _encoder = encoder;
            _translationManager = translationManager;
        }

        #region Public methods

        #region Normal authentication process

        [HttpGet]
        public ActionResult Index()
        {
            var authenticatedUser = this.GetAuthenticatedUser(); 
            if (authenticatedUser == null ||
            !authenticatedUser.IsAuthenticated())
            {
                TranslateView("en");
                return View();
            }

            // TODO : Redirect to user information page
            return null;
        }

        [HttpPost]
        public ActionResult LocalLogin(AuthorizeViewModel authorizeViewModel)
        {
            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException("authorizeViewModel");
            }

            
            return null;
        }

        [HttpPost]
        public ActionResult ExternalLogin(string provider)
        {
            return null;
        }

        #endregion

        #region Authentication process which follows OPENID

        [HttpGet]
        public ActionResult OpenId(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
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
                throw new ArgumentNullException("authorizeOpenId");    
            }

            if (string.IsNullOrWhiteSpace(authorizeOpenId.Code))
            {
                throw new ArgumentNullException("Code");
            }

            var code = _encoder.Decode(authorizeOpenId.Code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            try
            {
                var authenticationManager = this.GetAuthenticationManager();
                var claims = new List<Claim>();
                var actionResult = _authenticateActions.LocalOpenIdUserAuthentication(authorizeOpenId.ToParameter(),
                    request.ToParameter(),
                    authorizeOpenId.Code,
                    out claims);

                var claimIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    },
                    claimIdentity
                );

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
            return View(authorizeOpenId);
        }

        [HttpPost]
        public ActionResult ExternalLoginOpenId(string provider, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            // Add the information into the cookie
            var id = Guid.NewGuid().ToString();
            var name = string.Format(ExternalAuthenticateCookieName, id);
            var cookie = new HttpCookie(name)
            {
                Value = code,                
                Expires = DateTime.UtcNow.AddMinutes(5),
                
            };
            Response.Cookies.Add(cookie);

            return new ChallengeResult(provider,
                Url.Action("LoginCallbackOpenId", "Authenticate",
                new { code = id }));
        }
        
        public async Task<ActionResult> LoginCallbackOpenId(string code, string error)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode, 
                    string.Format(ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            // Retrieve the request from the cookie.
            var context = HttpContext.GetOwinContext();
            var information = context.Request.Cookies[string.Format(ExternalAuthenticateCookieName, code)];
            if (information == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode, ErrorDescriptions.TheRequestCannotBeExtractedFromTheCookie);
            }
            
            var authenticationManager = context.Authentication;
            var loginInformation = await authenticationManager.GetExternalLoginInfoAsync();
            if (loginInformation == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheLoginInformationCannotBeExtracted);
            }
            
            var decodedRequest = _encoder.Decode(code);
            var authorizationRequest = _protector.Decrypt<AuthorizationRequest>(decodedRequest);
            // var provider = loginInformation.Login.LoginProvider;
            var claims = loginInformation.ExternalIdentity.Claims;
            var actionResult = _authenticateActions.ExternalOpenIdUserAuthentication(claims.ToList(),
                authorizationRequest.ToParameter(),
                code); 
            var result = this.CreateRedirectionFromActionResult(actionResult,
                authorizationRequest);
            if (actionResult != null)
            {
                return result;
            }

            return RedirectToAction("Index", "Authenticate", new { code = code });
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

        #region Private classes

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
            }

            public string LoginProvider { get; private set; }

            public string RedirectUri { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties
                {
                    RedirectUri = RedirectUri
                };
                
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}