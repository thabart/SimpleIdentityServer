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

using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.Account;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Startup.ViewModels;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Startup.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountActions _accountActions;

        public AccountController(IAccountActions accountActions)
        {
            _accountActions = accountActions;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            if (authenticatedUser != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(UpdateResourceOwnerViewModel updateResourceOwnerViewModel)
        {
            if (updateResourceOwnerViewModel == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerViewModel));
            }

            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            if (authenticatedUser != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            await _accountActions.AddResourceOwner(new AddUserParameter
            {
                Login = updateResourceOwnerViewModel.Login,
                Password = updateResourceOwnerViewModel.Password
            }).ConfigureAwait(false);

            return RedirectToAction("Index", "Authenticate");
        }
    }
}