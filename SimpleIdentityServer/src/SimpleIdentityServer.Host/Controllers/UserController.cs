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
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Api.Controllers
{
    [Authorize]
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
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Consent()
        {
            return GetConsents();
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            var user = _userActions.GetUser(authenticatedUser);
            var viewModel = new ResourceOwnerViewModel
            {
                Name = user.Name,
                Password = user.Password,
                Roles = user.Roles
            };
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(UpdateResourceOwnerViewModel viewModel)
        {
            return RedirectToAction("Edit");
        }

        [HttpGet]
        public ActionResult Callback()
        {
            var query = Request.Query;
            var viewModel = new CallbackViewModel
            {
                IdentityToken = query[Core.Constants.StandardAuthorizationResponseNames.IdTokenName],
                AccessToken = query[Core.Constants.StandardAuthorizationResponseNames.AccessTokenName],
                State = query[Core.Constants.StandardAuthorizationResponseNames.StateName]
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Consent(string id)
        {
            if (!_userActions.DeleteConsent(id))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return GetConsents();
            }

            return RedirectToAction("Consent");
        }

        #endregion

        #region Private methods

        private ActionResult GetConsents()
        {
            var authenticatedUser = this.GetAuthenticatedUser();
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

        #endregion
    }
}