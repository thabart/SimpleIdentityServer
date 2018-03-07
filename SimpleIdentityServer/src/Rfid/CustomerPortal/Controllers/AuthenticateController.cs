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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CustomerPortal.Extensions;
using System.Linq;
using SimpleIdentityServer.Rfid.Website.ViewModels;
using Microsoft.Extensions.WebEncoders;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Security.Claims;

namespace CustomerPortal.Controllers
{
    public class AuthenticateController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            var providers = HttpContext.Authentication.GetAuthenticationSchemes()
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProviderViewModel
                {
                    AuthenticationScheme = x.AuthenticationScheme,
                    DisplayName = x.DisplayName
                });
            var viewModel = new LoginViewModel
            {
                ExternalProviders = providers,
                ReturnUrl = returnUrl
            };
            return View(viewModel);
        }

        public async Task ExternalProvider(string provider, string returnUrl)
        {
            if (returnUrl != null)
            {
                returnUrl = UrlEncoder.Default.UrlEncode(returnUrl);
            }

            returnUrl = "/Authenticate/ExternalLoginCallback?returnUrl=" + returnUrl;
            await HttpContext.Authentication.ChallengeAsync(provider, new AuthenticationProperties()
            {
                RedirectUri = returnUrl,
                Items = { { "scheme", provider } }
            });
        }

        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            // 1. Check parameters
            var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(Constants.ExternalCookieName);
            var tempUser = info?.Principal;
            if (tempUser == null)
            {
                throw new Exception("External authentication error");
            }

            if (!info.Properties.Items.Any(p => p.Key == "scheme"))
            {
                throw new Exception("the scheme cannot be retrieved");
            }

            // 2. Check subject claim exists.
            var subjectClaim = tempUser.Claims.FirstOrDefault(c => c.Type == SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            if (subjectClaim == null)
            {
                subjectClaim = tempUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            }

            if (subjectClaim == null)
            {
                throw new Exception("the subject cannot be retrieved");
            }

            // 3. Check card exists & retrieve the user
            var scheme = info.Properties.Items["scheme"];
            if (scheme == Constants.RfidProvider)
            {

            }

            var identity = new ClaimsIdentity(tempUser.Claims, Constants.CookieName);
            var user = new ClaimsPrincipal(identity);
            await HttpContext.Authentication.SignInAsync(Constants.CookieName, user);
            await HttpContext.Authentication.SignOutAsync(Constants.ExternalCookieName);
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/Account");
        }

        public async Task<ActionResult> Logout()
        {
            var authenticationManager = this.GetAuthenticationManager();
            await authenticationManager.SignOutAsync(Constants.CookieName);
            return RedirectToAction("Index", "Authenticate");
        }
    }
}
