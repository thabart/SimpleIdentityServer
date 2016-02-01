using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;

using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Translation;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IConsentActions _consentActions;

        private readonly IProtector _protector;


        private readonly IEncoder _encoder;

        private readonly ITranslationManager _translationManager;

        public ConsentController(
            IConsentActions consentActions,
            IProtector protector,
            IEncoder encoder,
            ITranslationManager translationManager)
        {
            _consentActions = consentActions;
            _protector = protector;
            _encoder = encoder;
            _translationManager = translationManager;
        }
        
        public ActionResult Index(string code)
        {
            var user = Request.HttpContext.User;
            code = _encoder.Decode(code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            var client = new Client();
            var scopes = new List<Scope>();
            var claims = new List<string>();
            var authenticatedUser = this.GetAuthenticatedUser();
            var actionResult = _consentActions.DisplayConsent(request.ToParameter(),
                authenticatedUser, 
                out client, 
                out scopes, 
                out claims);

            var result = this.CreateRedirectionFromActionResult(actionResult, request);
            if (result != null)
            {
                return result;
            }

            TranslateConsentScreen(request.ui_locales);
            var viewModel = new ConsentViewModel
            {
                ClientDisplayName = client.ClientName,
                AllowedScopeDescriptions = !scopes.Any() ? new List<string>() : scopes.Select(s => s.Description).ToList(),
                AllowedIndividualClaims = claims,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                TosUri = client.TosUri,
                Code = code
            };
            return View(viewModel);
        }
        
        public ActionResult Confirm(string code)
        {
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            var parameter = request.ToParameter();
            var authenticatedUser = this.GetAuthenticatedUser();
            var actionResult =_consentActions.ConfirmConsent(parameter,
                authenticatedUser);

            return this.CreateRedirectionFromActionResult(actionResult,
                request);
        }

        /// <summary>
        /// Action executed when the user refuse the consent.
        /// It redirects to the callback without passing the authorization code in parameter.
        /// </summary>
        /// <param name="code">Encrypted & signed authorization request</param>
        /// <returns>Redirect to the callback url.</returns>
        public ActionResult Cancel(string code)
        {
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            return Redirect(request.redirect_uri);
        }

        private void TranslateConsentScreen(string uiLocales)
        {
            // Retrieve the translation and store them in a ViewBag
            var translations = _translationManager.GetTranslations(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                Core.Constants.StandardTranslationCodes.ScopesCode,
                Core.Constants.StandardTranslationCodes.CancelCode,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                Core.Constants.StandardTranslationCodes.Tos
            });
            ViewBag.Translations = translations;
        }
    }
}