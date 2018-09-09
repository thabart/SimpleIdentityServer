using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Controllers.Website;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.UserManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UserManagement.Controllers
{
    using Core;

    [Area("UserManagement")]
    [Authorize("Connected")]
    public class UserController : BaseController
    {
        private const string DefaultLanguage = "en";
        private readonly IUserActions _userActions;
        private readonly IProfileActions _profileActions;
        private readonly ITranslationManager _translationManager;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IUrlHelper _urlHelper;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
        private readonly UserManagementOptions _userManagementOptions;

        #region Constructor

        public UserController(IUserActions userActions, IProfileActions profileActions, ITranslationManager translationManager, 
            IAuthenticationService authenticationService, IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler, UserManagementOptions userManagementOptions, 
            AuthenticateOptions options) : base(authenticationService, options)
        {
            _userActions = userActions;
            _profileActions = profileActions;
            _translationManager = translationManager;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _userManagementOptions = userManagementOptions;
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
            Check();
        }

        #endregion

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Consent()
        {
            await SetUser().ConfigureAwait(false);
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

        [HttpGet("User/Edit")]
        [HttpGet("User/UpdateCredentials")]
        [HttpGet("User/UpdateTwoFactor")]
        public async Task<IActionResult> Edit()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            return await GetEditView(authenticatedUser).ConfigureAwait(false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCredentials(UpdateResourceOwnerCredentialsViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }
            
            // 1. Validate the view model.
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            viewModel.Validate(ModelState);
            if (!ModelState.IsValid)
            {
                return await GetEditView(authenticatedUser).ConfigureAwait(false);
            }

            // 2. Create a new user if he doesn't exist or update the credentials.
            var resourceOwner = await _userActions.GetUser(authenticatedUser).ConfigureAwait(false);
            var subject = authenticatedUser.GetSubject();
            await _userActions.UpdateCredentials(subject, viewModel.Password).ConfigureAwait(false);
            ViewBag.IsUpdated = true;
            return await GetEditView(authenticatedUser).ConfigureAwait(false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTwoFactor(UpdateTwoFactorAuthenticatorViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }
            
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            await _userActions.UpdateTwoFactor(authenticatedUser.GetSubject(), viewModel.SelectedTwoFactorAuthType).ConfigureAwait(false);
            ViewBag.IsUpdated = true;
            return await GetEditView(authenticatedUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Display the profiles linked to the user account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var actualScheme = authenticatedUser.Identity.AuthenticationType;
            var profiles = await _profileActions.GetProfiles(authenticatedUser.GetSubject()).ConfigureAwait(false);
            var authenticationSchemes = (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(a => !string.IsNullOrWhiteSpace(a.DisplayName));
            var viewModel = new ProfileViewModel();
            if (profiles != null && profiles.Any())
            {
                foreach (var profile in profiles)
                {
                    var record = new IdentityProviderViewModel(profile.Issuer, profile.Subject);
                    viewModel.LinkedIdentityProviders.Add(record);
                }
            }
            
            viewModel.UnlinkedIdentityProviders = authenticationSchemes.Where(a => profiles != null && !profiles.Any(p => p.Issuer == a.Name && a.Name != actualScheme))
                .Select(p => new IdentityProviderViewModel(p.Name)).ToList();
            return View("Profile", viewModel);
        }

        /// <summary>
        /// Link an external account to the local one.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Link(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var redirectUrl = _urlHelper.AbsoluteAction("LinkCallback", "User", new { area = "UserManagement" });
            await _authenticationService.ChallengeAsync(HttpContext, provider, new AuthenticationProperties()
            {
                RedirectUri = redirectUrl
            }).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Callback operation used to link an external account to the local one.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LinkCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, string.Format(Core.Errors.ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            try
            {
                var authenticatedUser = await SetUser().ConfigureAwait(false);
                var externalClaims = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName).ConfigureAwait(false);                
                var resourceOwner = await _profileActions.Link(authenticatedUser.GetSubject(), externalClaims.GetSubject(), externalClaims.Identity.AuthenticationType, false).ConfigureAwait(false);
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties()).ConfigureAwait(false);
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }
            catch (ProfileAssignedAnotherAccountException)
            {
                return RedirectToAction("LinkProfileConfirmation");
            }
            catch (Exception)
            {
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties()).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Confirm to link the external account to this local account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LinkProfileConfirmation()
        {
            var externalClaims = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName).ConfigureAwait(false);
            if (externalClaims == null ||
                externalClaims.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }

            await SetUser().ConfigureAwait(false);
            var authenticationType = ((ClaimsIdentity)externalClaims.Identity).AuthenticationType;
            var viewModel = new LinkProfileConfirmationViewModel(authenticationType);
            return View(viewModel);
        }
        
        /// <summary>
        /// Force to link the external account to the local one.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConfirmProfileLinking()
        {
            var externalClaims = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName).ConfigureAwait(false);
            if (externalClaims == null ||
                externalClaims.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await SetUser().ConfigureAwait(false);
            try
            {
                await _profileActions.Link(authenticatedUser.GetSubject(), externalClaims.GetSubject(), externalClaims.Identity.AuthenticationType, true).ConfigureAwait(false);
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }
            finally
            {
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties()).ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Unlink the external account.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Unlink(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            try
            {
                await _profileActions.Unlink(authenticatedUser.GetSubject(), id).ConfigureAwait(false);
            }
            catch (IdentityServerException ex)
            {
                return RedirectToAction("Index", "Error", new { code = ex.Code, message = ex.Message, area = "Shell" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { code = ErrorCodes.InternalError, message = ex.Message, area = "Shell" });
            }

            return await Profile().ConfigureAwait(false);
        }

        #endregion

        #region Private methods

        private async Task<IActionResult> GetEditView(ClaimsPrincipal authenticatedUser)
        {
            var resourceOwner = await _userActions.GetUser(authenticatedUser).ConfigureAwait(false);
            UpdateResourceOwnerViewModel viewModel = null;
            if (resourceOwner == null)
            {
                viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication, authenticatedUser.GetSubject(), authenticatedUser.Claims, false);
                return View("Edit", viewModel);
            }

            viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication, authenticatedUser.GetSubject(), resourceOwner.Claims, true);
            viewModel.IsLocalAccount = true;
            return View("Edit", viewModel);
        }

        private async Task<ActionResult> GetConsents()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var consents = await _userActions.GetConsents(authenticatedUser).ConfigureAwait(false);
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
                        AllowedIndividualClaims = claims ?? new List<string>(),
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
                Constants.StandardTranslationCodes.LoginCode,
                Constants.StandardTranslationCodes.EditResourceOwner,
                Constants.StandardTranslationCodes.NameCode,
                Constants.StandardTranslationCodes.YourName,
                Constants.StandardTranslationCodes.PasswordCode,
                Constants.StandardTranslationCodes.YourPassword,
                Constants.StandardTranslationCodes.Email,
                Constants.StandardTranslationCodes.YourEmail,
                Constants.StandardTranslationCodes.ConfirmCode,
                Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                Constants.StandardTranslationCodes.UserIsUpdated,
                Constants.StandardTranslationCodes.Phone,
                Constants.StandardTranslationCodes.HashedPassword,
                Constants.StandardTranslationCodes.CreateResourceOwner,
                Constants.StandardTranslationCodes.Credentials,
                Constants.StandardTranslationCodes.RepeatPassword,
                Constants.StandardTranslationCodes.Claims,
                Constants.StandardTranslationCodes.UserIsCreated,
                Constants.StandardTranslationCodes.TwoFactor,
                Constants.StandardTranslationCodes.NoTwoFactorAuthenticator,
                Constants.StandardTranslationCodes.NoTwoFactorAuthenticatorSelected
            }).ConfigureAwait(false);

            ViewBag.Translations = translations;
        }

        private UpdateResourceOwnerViewModel BuildViewModel(string twoFactorAuthType, string subject, IEnumerable<Claim> claims, bool isLocalAccount)
        {
            var editableClaims = new Dictionary<string, string>();
            var notEditableClaims = new Dictionary<string, string>();
            foreach(var claim in claims)
            {
                if (Core.Jwt.Constants.NotEditableResourceOwnerClaimNames.Contains(claim.Type))
                {
                    notEditableClaims.Add(claim.Type, claim.Value);
                }
                else
                {
                    editableClaims.Add(claim.Type, claim.Value);
                }
            }
            
            var result = new UpdateResourceOwnerViewModel(subject, editableClaims, notEditableClaims, isLocalAccount);
            result.SelectedTwoFactorAuthType = twoFactorAuthType;
            result.TwoFactorAuthTypes = _twoFactorAuthenticationHandler.GetAll().Select(s => s.Name).ToList();
            return result;
        }

        /// <summary>
        /// Check the parameters.
        /// </summary>
        private void Check()
        {
            if (_userManagementOptions.CreateScimResourceWhenAccountIsAdded && (_userManagementOptions.AuthenticationOptions == null ||
                string.IsNullOrWhiteSpace(_userManagementOptions.AuthenticationOptions.AuthorizationWellKnownConfiguration) ||
                string.IsNullOrWhiteSpace(_userManagementOptions.AuthenticationOptions.ClientId) ||
                string.IsNullOrWhiteSpace(_userManagementOptions.AuthenticationOptions.ClientSecret) ||
                string.IsNullOrWhiteSpace(_userManagementOptions.ScimBaseUrl)))
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.InternalError, Core.Errors.ErrorDescriptions.TheScimConfigurationMustBeSpecified);
            }
        }

        #endregion
    }
}
