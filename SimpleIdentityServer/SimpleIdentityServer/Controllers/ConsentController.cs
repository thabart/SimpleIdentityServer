using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Api.Parsers;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Protector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ActionResult = System.Web.Mvc.ActionResult;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IConsentActions _consentActions;

        private readonly IProtector _protector;

        private readonly IActionResultParser _actionResultParser;

        private readonly IEncoder _encoder;

        public ConsentController(
            IConsentActions consentActions,
            IProtector protector,
            IActionResultParser actionResultParser,
            IEncoder encoder)
        {
            _consentActions = consentActions;
            _protector = protector;
            _actionResultParser = actionResultParser;
            _encoder = encoder;
        }
        
        public ActionResult Index(string code)
        {
            code = _encoder.Decode(code);
            var request = _protector.Decrypt<AuthorizationRequest>(code);
            var client = new Client();
            var scopes = new List<Scope>();
            _consentActions.DisplayConsent(request.ToParameter(), out client, out scopes);
            var viewModel = new ConsentViewModel
            {
                ClientDisplayName = client.DisplayName,
                AllowedScopeDescriptions = scopes.Select(s => s.Description).ToList(),
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
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var parameters = _actionResultParser.GetRedirectionParameters(actionResult);
                var uri = new Uri(request.redirect_uri);
                var redirectUrl = this.CreateRedirectHttp(uri, parameters, parameter.ResponseMode);
                return Redirect(redirectUrl);
            }

            var actionInformation =
                _actionResultParser.GetControllerAndActionFromRedirectionActionResult(actionResult);
            if (actionInformation != null)
            {
                return RedirectToAction(
                    actionInformation.ActionName,
                    actionInformation.ControllerName,
                    actionInformation.RouteValueDictionary);
            }

            return null;
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
    }
}