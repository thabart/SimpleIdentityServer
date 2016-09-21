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
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize("Connected")]
    public class UserController : Controller
    {
        private readonly IUserActions _userActions;

        #region Constructor

        public UserController(IUserActions userActions)
        {
            _userActions = userActions;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var user = await GetCurrentUser();
            ViewBag.IsLocalAccount = user.IsLocalAccount;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Consent()
        {
            var user = await GetCurrentUser();
            ViewBag.IsLocalAccount = user.IsLocalAccount;
            return await GetConsents();
        }

        [HttpPost]
        public async Task<ActionResult> Consent(string id)
        {
            if (!_userActions.DeleteConsent(id))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return await GetConsents();
            }

            return RedirectToAction("Consent");
        }

        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            var user = await GetCurrentUser();
            if (!user.IsLocalAccount)
            {
                return RedirectToAction("Index");
            }

            ViewBag.IsLocalAccount = user.IsLocalAccount;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Edit(UpdateResourceOwnerViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            var user = await GetCurrentUser();
            if (!user.IsLocalAccount)
            {
                return RedirectToAction("Index");
            }

            var parameter = viewModel.ToParameter();
            parameter.Id = user.Id;
            _userActions.UpdateUser(parameter);
            return RedirectToAction("Edit");
        }

        [HttpGet]
        public async Task<ActionResult> Simulator()
        {
            var user = await GetCurrentUser();
            ViewBag.IsLocalAccount = user.IsLocalAccount;
            ViewBag.Url = string.Format("{0}://{1}", HttpContext.Request.Scheme, HttpContext.Request.Host.Value);
            return View();
        }

        [HttpGet]
        public ActionResult Callback()
        {
            var p = HttpContext.Request.Path;
            var path = Request.Path;
            var query = Request.Query;
            var viewModel = new CallbackViewModel
            {
                IdentityToken = query[Core.Constants.StandardAuthorizationResponseNames.IdTokenName],
                AccessToken = query[Core.Constants.StandardAuthorizationResponseNames.AccessTokenName],
                State = query[Core.Constants.StandardAuthorizationResponseNames.StateName]
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> Confirm()
        {
            var user = await this.GetAuthenticatedUser();
            _userActions.ConfirmUser(user);
            return RedirectToAction("Index");
        }

        #endregion

        #region Private methods

        private async Task<ActionResult> GetConsents()
        {
            var authenticatedUser = await this.GetAuthenticatedUser();
            var consents = _userActions.GetConsents(authenticatedUser);
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

            return View(result);
        }

        private async Task<ResourceOwner> GetCurrentUser()
        {
            var authenticatedUser = await this.GetAuthenticatedUser();
            return  _userActions.GetUser(authenticatedUser);
        }

        #endregion
    }
}