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
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Events;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Startup.ViewModels;
using System;
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
        private readonly IEventPublisher _eventPublisher;
        private readonly IEventAggregateRepository _eventAggregateRepository;

        public ConsentController(
            IConsentActions consentActions,
            IDataProtectionProvider dataProtectionProvider,
            ITranslationManager translationManager,
            IEventPublisher eventPublisher,
            IEventAggregateRepository eventAggregateRepository)
        {
            _consentActions = consentActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _translationManager = translationManager;
            _eventPublisher = eventPublisher;
            _eventAggregateRepository = eventAggregateRepository;
        }
        
        public async Task<ActionResult> Index(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var client = new Core.Models.Client();
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            var actionResult = await _consentActions.DisplayConsent(request.ToParameter(),
                authenticatedUser).ConfigureAwait(false);

            var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult, request);
            if (result != null)
            {
                return result;
            }

            await TranslateConsentScreen(request.UiLocales).ConfigureAwait(false);
            var viewModel = new ConsentViewModel
            {
                ClientDisplayName = client.ClientName,
                AllowedScopeDescriptions = actionResult.Scopes == null ? new List<string>() : actionResult.Scopes.Select(s => s.Description).ToList(),
                AllowedIndividualClaims = actionResult.AllowedClaims == null ? new List<string>() : actionResult.AllowedClaims,
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
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            var actionResult = await _consentActions.ConfirmConsent(parameter,
                authenticatedUser).ConfigureAwait(false);
            await LogConsentAccepted(actionResult, parameter.ProcessId).ConfigureAwait(false);
            return this.CreateRedirectionFromActionResult(actionResult,
                request);
        }

        /// <summary>
        /// Action executed when the user refuse the consent.
        /// It redirects to the callback without passing the authorization code in parameter.
        /// </summary>
        /// <param name="code">Encrypted & signed authorization request</param>
        /// <returns>Redirect to the callback url.</returns>
        public async Task<ActionResult> Cancel(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            await LogConsentRejected(request.ProcessId).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
            ViewBag.Translations = translations;
        }

        private async Task LogConsentAccepted(Core.Results.ActionResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            var evtAggregate = await GetLastEventAggregate(processId).ConfigureAwait(false);
            if (evtAggregate == null)
            {
                return;
            }

            _eventPublisher.Publish(new ConsentAccepted(Guid.NewGuid().ToString(), processId, act, evtAggregate.Order + 1));
        }

        private async Task LogConsentRejected(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            var evtAggregate = await GetLastEventAggregate(processId).ConfigureAwait(false);
            if (evtAggregate == null)
            {
                return;
            }

            _eventPublisher.Publish(new ConsentRejected(Guid.NewGuid().ToString(), processId, evtAggregate.Order + 1));
        }

        private async Task<EventAggregate> GetLastEventAggregate(string aggregateId)
        {
            var events = (await _eventAggregateRepository.GetByAggregate(aggregateId).ConfigureAwait(false)).OrderByDescending(e => e.Order);
            if (events == null || !events.Any())
            {
                return null;
            }

            return events.First();
        }
    }
}