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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Startup.Extensions;
using SimpleIdentityServer.Startup.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Startup.Controllers
{
    [Authorize("Connected")]
    public class UserController : BaseController
    {
        private const string DefaultLanguage = "en";
        private readonly IUserActions _userActions;
        private readonly ITranslationManager _translationManager;

        #region Constructor

        public UserController(
            IUserActions userActions,
            ITranslationManager translationManager,
            IAuthenticationService authenticationService) : base(authenticationService, userActions)
        {
            _userActions = userActions;
            _translationManager = translationManager;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Consent()
        {
            await SetUser();
            return await GetConsents();
        }

        [HttpPost]
        public async Task<ActionResult> Consent(string id)
        {
            if (!await _userActions.DeleteConsent(id))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return await GetConsents();
            }

            return RedirectToAction("Consent");
        }

        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            var user = await SetUser();
            if (user.Value != null && !user.Value.IsLocalAccount)
            {
                return RedirectToAction("Index");
            }

            await TranslateUserEditView(DefaultLanguage);
            ViewBag.IsUpdated = false;
            string subject = string.Empty,
                email = string.Empty,
                phoneNumber = string.Empty,
                name = string.Empty;
            if(user.Value.Claims != null)
            {
                var subjectClaim = user.Value.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                var emailClaim = user.Value.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email);
                var phoneNumberClaim = user.Value.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
                var nameClaim = user.Value.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name);
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
                Password = user.Value.Password,
                TwoAuthenticationFactor = user.Value.TwoFactorAuthentication,
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
            var user = await SetUser();
            var subject = user.Key.GetSubject();
            if (!user.Value.IsLocalAccount)
            {
                throw new IdentityServerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheResourceOwnerIsNotALocalAccount);
            }

            await TranslateUserEditView(DefaultLanguage);
            // 2. Validate the view model
            if (!ModelState.IsValid)
            {
                ViewBag.IsUpdated = false;
                return View(viewModel);
            }

            // 3. Update the resource owner
            var parameter = viewModel.ToParameter();
            foreach(var newClaim in user.Value.Claims.Where(uc => !parameter.Claims.Any(pc => pc.Type == uc.Type)))
            {
                parameter.Claims.Add(newClaim);
            }

            parameter.Login = subject;
            await _userActions.UpdateUser(parameter);

            // 4. Returns translated view
            ViewBag.IsUpdated = true;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> Simulator()
        {
            await SetUser();
            ViewBag.Url = string.Format("{0}://{1}", HttpContext.Request.Scheme, HttpContext.Request.Host.Value);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Callback()
        {
            await SetUser();
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
            var user = await _authenticationService.GetAuthenticatedUser(this, Constants.CookieName);
            await _userActions.ConfirmUser(user);
            return RedirectToAction("Index");
        }

        #endregion

        #region Private methods

        private async Task<ActionResult> GetConsents()
        {
            var authenticatedUser = await SetUser();
            var consents = await _userActions.GetConsents(authenticatedUser.Key);
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
            });

            ViewBag.Translations = translations;
        }

        #endregion
    }
}