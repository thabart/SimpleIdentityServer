using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Controllers.Website;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.ViewModels;
using SimpleIdentityServer.UserManagement.Extensions;
using SimpleIdentityServer.UserManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize("Connected")]
    public class UserController : BaseController
    {
        private const string DefaultLanguage = "en";
        private readonly IUserActions _userActions;
        private readonly ITranslationManager _translationManager;

        #region Constructor

        public UserController(IUserActions userActions, ITranslationManager translationManager, IAuthenticationService authenticationService, AuthenticateOptions options) : base(authenticationService, options)
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
            var authenticatedUser = await SetUser();
            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            await TranslateUserEditView(DefaultLanguage);
            UpdateResourceOwnerViewModel viewModel = null;
            ViewBag.IsUpdated = false;
            if (resourceOwner == null)
            {
                viewModel = BuildViewModel(authenticatedUser.Claims);
                return View(viewModel);
            }

            viewModel = BuildViewModel(User.Claims);
            viewModel.Password = resourceOwner.Password;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateResourceOwnerViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            await TranslateUserEditView(DefaultLanguage);
            var authenticatedUser = await SetUser();
            if (!ModelState.IsValid)
            {
                ViewBag.IsUpdated = false;
                return View(viewModel);
            }

            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            var subject = authenticatedUser.GetSubject();
            if (resourceOwner == null)
            {
                var record = viewModel.ToAddUserParameter();
                await _userActions.AddUser(record);
            }
            else
            {
                var parameter = viewModel.ToParameter();
                foreach (var newClaim in resourceOwner.Claims.Where(uc => !parameter.Claims.Any(pc => pc.Type == uc.Type)))
                {
                    parameter.Claims.Add(newClaim);
                }

                parameter.Login = subject;
                await _userActions.UpdateUser(parameter);

            }

            ViewBag.IsUpdated = true;
            return View(viewModel);
        }

        #endregion

        #region Private methods

        private async Task<ActionResult> GetConsents()
        {
            var authenticatedUser = await SetUser();
            var consents = await _userActions.GetConsents(authenticatedUser);
            var result = new List<ConsentViewModel>();
            if (consents != null)
            {
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

        private static UpdateResourceOwnerViewModel BuildViewModel(IEnumerable<Claim> claims)
        {
            string subject = string.Empty,
                email = string.Empty,
                phoneNumber = string.Empty,
                name = string.Empty;
            if (claims != null)
            {
                var subjectClaim = claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                var emailClaim = claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email);
                var phoneNumberClaim = claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
                var nameClaim = claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name);
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

            return new UpdateResourceOwnerViewModel
            {
                Login = subject,
                Email = email,
                Name = name,
                // Password = user.Value.Password,
                // TwoAuthenticationFactor = user.Value.TwoFactorAuthentication,
                PhoneNumber = phoneNumber
            };
        }

        #endregion
    }
}
