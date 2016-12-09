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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Startup.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Startup.Controllers
{
    [Authorize("Connected")]
    public class ConsentController : Controller
    {
        private readonly IConsentActions _consentActions;
        private readonly IDataProtector _dataProtector;
        private readonly ITranslationManager _translationManager;

        public ConsentController(
            IConsentActions consentActions,
            IDataProtectionProvider dataProtectionProvider,
            ITranslationManager translationManager)
        {
            _consentActions = consentActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _translationManager = translationManager;
        }
        
        public async Task<ActionResult> Index(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var client = new Core.Models.Client();
            var scopes = new List<Scope>();
            var claims = new List<string>();
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName);
            var actionResult = await _consentActions.DisplayConsent(request.ToParameter(),
                authenticatedUser);

            var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult, request);
            if (result != null)
            {
                return result;
            }

            await TranslateConsentScreen(request.UiLocales);
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
        
        public async Task<ActionResult> Confirm(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var parameter = request.ToParameter();
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName);
            var actionResult = await _consentActions.ConfirmConsent(parameter,
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
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            return Redirect(request.RedirectUri);
        }

        private async Task TranslateConsentScreen(string uiLocales)
        {
            // Retrieve the translation and store them in a ViewBag
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
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