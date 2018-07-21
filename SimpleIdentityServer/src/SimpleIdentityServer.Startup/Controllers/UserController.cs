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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Startup.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdentityServer.Startup.Extensions;
using SimpleIdentityServer.Core.Extensions;

namespace SimpleIdentityServer.Startup.Controllers
{
    [Authorize("Connected")]
    public class UserController : Controller
    {
        private const string DefaultLanguage = "en";
        private readonly IUserActions _userActions;
        private readonly ITranslationManager _translationManager;

        #region Constructor

        public UserController(
            IUserActions userActions,
            ITranslationManager translationManager)
        {
            _userActions = userActions;
            _translationManager = translationManager;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var user = await GetCurrentUser().ConfigureAwait(false);
            ViewBag.IsLocalAccount = user.IsLocalAccount;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Consent()
        {
            var user = await GetCurrentUser().ConfigureAwait(false);
            ViewBag.IsLocalAccount = user.IsLocalAccount;
            return await GetConsents().ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<ActionResult> Consent(string id)
        {
            if (!await _userActions.DeleteConsent(id).ConfigureAwait(false))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return await GetConsents().ConfigureAwait(false);
            }

            return RedirectToAction("Consent");
        }

        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            var user = await GetCurrentUser().ConfigureAwait(false);
            if (!await SetUserEditViewBag(user).ConfigureAwait(false))
            {
                return RedirectToAction("Index");
            }

            ViewBag.IsUpdated = false;
            string subject = string.Empty,
                email = string.Empty,
                phoneNumber = string.Empty,
                name = string.Empty;
            if(user.Claims != null)
            {
                var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                var emailClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email);
                var phoneNumberClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
                var nameClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name);
                if (subjectClaim != null)
                {
                    subject = subjectClaim.Value;
                }

                if (emailClaim != null)
                {
                    email = emailClaim.Value;
                }

                if (phoneNumberClaim != null)
                {
                    phoneNumber = phoneNumberClaim.Value;
                }

                if (nameClaim != null)
                {
                    name = nameClaim.Value;
                }
            }

            return View(new UpdateResourceOwnerViewModel
            {
                Login = subject,
                Email = email,
                Name = name,
                Password = user.Password,
                TwoAuthenticationFactor = user.TwoFactorAuthentication,
                PhoneNumber = phoneNumber
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateResourceOwnerViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            // 1. Set view bag
            var subject = (await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false)).GetSubject();
            var user = await GetCurrentUser().ConfigureAwait(false);
            if (!await SetUserEditViewBag(user).ConfigureAwait(false))
            {
                throw new IdentityServerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheResourceOwnerIsNotALocalAccount);
            }

            // 2. Validate the view model
            if (!ModelState.IsValid)
            {
                ViewBag.IsUpdated = false;
                return View(viewModel);
            }

            // 3. Update the resource owner
            var parameter = viewModel.ToParameter();
            foreach(var newClaim in user.Claims.Where(uc => !parameter.Claims.Any(pc => pc.Type == uc.Type)))
            {
                parameter.Claims.Add(newClaim);
            }

            parameter.Login = subject;
            await _userActions.UpdateUser(parameter).ConfigureAwait(false);

            // 4. Returns translated view
            ViewBag.IsUpdated = true;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> Simulator()
        {
            var user = await GetCurrentUser().ConfigureAwait(false);
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
            var user = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            await _userActions.ConfirmUser(user).ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        #endregion

        #region Private methods

        private async Task<ActionResult> GetConsents()
        {
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            var consents = await _userActions.GetConsents(authenticatedUser).ConfigureAwait(false);
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
            var authenticatedUser = await this.GetAuthenticatedUser(Constants.CookieName).ConfigureAwait(false);
            return await _userActions.GetUser(authenticatedUser).ConfigureAwait(false);
        }

        private async Task<bool> SetUserEditViewBag(ResourceOwner user)
        {
            if (!user.IsLocalAccount)
            {
                return false;
            }

            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            ViewBag.IsLocalAccount = user.IsLocalAccount;
            return true;
        }

        private async Task TranslateUserEditView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales, new List<string>
            {
                Core.Constants.StandardTranslationCodes.LoginCode,
                Core.Constants.StandardTranslationCodes.EditResourceOwner,
                Core.Constants.StandardTranslationCodes.NameCode,
                Core.Constants.StandardTranslationCodes.YourName,
                Core.Constants.StandardTranslationCodes.PasswordCode,
                Core.Constants.StandardTranslationCodes.YourPassword,
                Core.Constants.StandardTranslationCodes.Email,
                Core.Constants.StandardTranslationCodes.YourEmail,
                Core.Constants.StandardTranslationCodes.ConfirmCode,
                Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                Core.Constants.StandardTranslationCodes.UserIsUpdated,
                Core.Constants.StandardTranslationCodes.Phone,
                Core.Constants.StandardTranslationCodes.HashedPassword
            }).ConfigureAwait(false);

            ViewBag.Translations = translations;
        }

        #endregion
    }
}