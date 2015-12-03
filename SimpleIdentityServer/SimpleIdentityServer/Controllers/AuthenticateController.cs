using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Api.Parsers;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Exceptions;
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

        private readonly IActionResultParser _actionResultParser;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly ITranslationManager _translationManager;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IActionResultParser actionResultParser,
            IProtector protector,
            IEncoder encoder,
            ITranslationManager translationManager)
        {
            _authenticateActions = authenticateActions;
            _actionResultParser = actionResultParser;
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

            var actionInformation = _actionResultParser.GetControllerAndActionFromRedirectionActionResult(actionResult);
            if (actionInformation != null)
            {
                return RedirectToAction(
                    actionInformation.ActionName,
                    actionInformation.ControllerName,
                    actionInformation.RouteValueDictionary);
            }

            var translations = _translationManager.GetTranslations(request.ui_locales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.LoginCode,
                Core.Constants.StandardTranslationCodes.UserNameCode,
                Core.Constants.StandardTranslationCodes.PasswordCode,
                Core.Constants.StandardTranslationCodes.RememberMyLoginCode
            });

            var viewModel = new AuthorizeViewModel
            {
                Code = code
            };

            ViewBag.Translations = translations;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(AuthorizeViewModel authorize)
        {
            var code = _encoder.Decode(authorize.Code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            try
            {
                var authenticatedUser = this.GetAuthenticatedUser();
                var authenticationManager = this.GetAuthenticationManager();
                var claims = new List<Claim>();
                var actionResult = _authenticateActions.LocalUserAuthentication(authorize.ToParameter(),
                    request.ToParameter(),
                    authenticatedUser,
                    authorize.Code,
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

                var actionInformation =
                    _actionResultParser.GetControllerAndActionFromRedirectionActionResult(actionResult);
                if (actionInformation != null)
                {
                    return RedirectToAction(
                        actionInformation.ActionName,
                        actionInformation.ControllerName,
                        actionInformation.RouteValueDictionary);
                }

            }
            catch (IdentityServerAuthenticationException exception)
            {
                return View(authorize);
            }

            return View(authorize);
        }
    }
}