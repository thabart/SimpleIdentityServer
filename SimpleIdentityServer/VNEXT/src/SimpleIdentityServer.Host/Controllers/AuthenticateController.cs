using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
// using Microsoft.AspNet.Security.Cookies;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;

namespace SimpleIdentityServer.Host.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IAuthenticateActions _authenticateActions;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly ITranslationManager _translationManager;

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

        [HttpGet]
        public ActionResult Index(string code)
        {
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


                var claimIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                /*
                this.HttpContext.Response.SignIn(
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    },
                    claimIdentity
                );*/

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