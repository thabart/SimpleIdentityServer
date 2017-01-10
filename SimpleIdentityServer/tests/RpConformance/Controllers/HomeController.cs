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
using SimpleIdentityServer.Uma.Common;
using System.Security.Claims;
using RpConformance.Extensions;

namespace RpConformance.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var authenticatedUser = this.GetAuthenticatedUser();
            var identity = authenticatedUser.Identity as ClaimsIdentity;
            var permissions = identity.GetPermissions();
            return View(permissions);
        }

        public IActionResult Disconnect()
        {
            var authenticationManager = this.GetAuthenticationManager();
            authenticationManager.SignOutAsync(Constants.CookieWebApplicationName).Wait();
            return RedirectToAction("Index", "Authenticate");
        }
    }
}
