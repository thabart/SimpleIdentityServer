#region copyright
// Copyright 2016 Habart Thierry
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
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Rfid.Website.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Rfid.Website.Controllers
{
    [Authorize("Connected")]
    public class AccountController : Controller
    {
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;

        public AccountController(
            IGetConsentsOperation getConsentsOperation,
            IGetUserOperation getUserOperation,
            IRemoveConsentOperation removeConsentOperation)
        {
            _getConsentsOperation = getConsentsOperation;
            _getUserOperation = getUserOperation;
            _removeConsentOperation = removeConsentOperation;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Consent()
        {
            return View(await GetConsents());
        }

        [HttpPost]
        public async Task<ActionResult> Consent(string id)
        {
            if (!await _removeConsentOperation.Execute(id))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return View(await GetConsents());
            }

            return RedirectToAction("Consent");
        }

        private async Task<ResourceOwner> GetCurrentUser()
        {
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName);
            return await _getUserOperation.Execute(authenticatedUser);
        }

        private async Task<List<ConsentViewModel>> GetConsents()
        {
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName);
            var consents = await _getConsentsOperation.Execute(authenticatedUser);
            var result = new List<ConsentViewModel>();
            foreach (var consent in consents)
            {
                var client = consent.Client;
                var scopes = consent.GrantedScopes;
                var claims = consent.Claims;
                var viewModel = new ConsentViewModel
                {
                    Id = consent.Id,
                    ClientDisplayName = client == null ? string.Empty : client.ClientName,
                    AllowedScopeDescriptions = scopes == null || !scopes.Any() ?
                        new List<string>() :
                        scopes.Select(g => g.Description).ToList(),
                    AllowedIndividualClaims = claims == null ? new List<string>() : claims,
                    LogoUri = client == null ? string.Empty : client.LogoUri,
                    PolicyUri = client == null ? string.Empty : client.PolicyUri,
                    TosUri = client == null ? string.Empty : client.TosUri
                };

                result.Add(viewModel);
            }

            return result;
        }
    }
}
