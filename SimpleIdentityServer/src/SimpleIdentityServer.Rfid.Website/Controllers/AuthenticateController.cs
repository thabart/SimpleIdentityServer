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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Rfid.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Rfid.Website.Controllers
{
    public class AuthenticateController : Controller
    {
        private const string DefaultLanguage = "en";
        private readonly IAuthenticateActions _authenticateActions;
        private readonly IDataProtector _dataProtector;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IDataProtectionProvider dataProtectionProvider,
            ITranslationManager translationManager)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _authenticateActions = authenticateActions;
        }

        public ActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel loginViewModel)
        {
            if (loginViewModel == null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            try
            {
                // 1. Authenticate the resource owner.
                var resourceOwner = _authenticateActions.LocalUserAuthentication(new LocalAuthenticationParameter
                {
                    Password = loginViewModel.Password,
                    UserName = loginViewModel.CardNumber
                });
                // 2. Store the claims into the cookie.
                var claims = resourceOwner.Claims;
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
                var subject = claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                var authenticationManager = this.GetAuthenticationManager();
                await SetLocalCookie(authenticationManager, claims);
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityServerAuthenticationException ex)
            {
                ModelState.AddModelError("invalid_credentials", ex.Message);
                return View(loginViewModel);
            }
        }

        [HttpGet]
        public async Task<ActionResult> OpenId(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName);
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var actionResult = _authenticateActions.AuthenticateResourceOwnerOpenId(
                request.ToParameter(),
                authenticatedUser,
                code);
            var result = this.CreateRedirectionFromActionResult(actionResult,
                request);
            if (result != null)
            {
                return result;
            }
            
            var viewModel = new LoginOpenIdViewModel
            {
                Code = code
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> OpenId(LoginOpenIdViewModel loginViewModel)
        {
            if (loginViewModel == null)
            {
                throw new ArgumentNullException(nameof(loginViewModel));
            }

            if (string.IsNullOrWhiteSpace(loginViewModel.Code))
            {
                throw new ArgumentNullException(nameof(loginViewModel.Code));
            }

            var uiLocales = DefaultLanguage;
            try
            {
                // 1. Decrypt the request
                var request = _dataProtector.Unprotect<AuthorizationRequest>(loginViewModel.Code);
                // 2. Retrieve the default language
                uiLocales = string.IsNullOrWhiteSpace(request.ui_locales) ? DefaultLanguage : request.ui_locales;
                // 3. Check the state of the view model
                if (!ModelState.IsValid)
                {
                    return View("OpenId", loginViewModel);
                }

                // 4. Local authentication
                var claims = new List<Claim>();
                var actionResult = _authenticateActions.LocalOpenIdUserAuthentication(new LocalAuthenticationParameter
                    {
                        Password = loginViewModel.Password,
                        UserName = loginViewModel.CardNumber
                    },
                    request.ToParameter(),
                    loginViewModel.Code,
                    out claims);
                var subject = claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;

                // 5. Authenticate the user by adding a cookie
                var authenticationManager = this.GetAuthenticationManager();
                await SetLocalCookie(authenticationManager, claims);

                // 6. Redirect the user agent
                var result = this.CreateRedirectionFromActionResult(actionResult,
                    request);
                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            // TranslateView(uiLocales);
            return View("OpenId", loginViewModel);
        }

        public async Task<ActionResult> Logout()
        {
            var authenticationManager = this.GetAuthenticationManager();
            await authenticationManager.SignOutAsync(Constants.CookieName);
            return RedirectToAction("Index", "Authenticate");
        }
                
        private async Task SetLocalCookie(AuthenticationManager authenticationManager, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Constants.CookieName);
            var principal = new ClaimsPrincipal(identity);
            await authenticationManager.SignInAsync(Constants.CookieName,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    IsPersistent = false
                });
        }
    }
}
