using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Exceptions;
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
        private readonly IProfileActions _profileActions;
        private readonly ITranslationManager _translationManager;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IUrlHelper _urlHelper;

        #region Constructor

        public UserController(IUserActions userActions, IProfileActions profileActions, ITranslationManager translationManager, 
            IAuthenticationService authenticationService, IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, AuthenticateOptions options) : base(authenticationService, options)
        {
            _userActions = userActions;
            _profileActions = profileActions;
            _translationManager = translationManager;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
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

        [HttpGet("User/Edit")]
        [HttpGet("User/UpdateClaims")]
        [HttpGet("User/UpdateCredentials")]
        public async Task<IActionResult> Edit()
        {
            var authenticatedUser = await SetUser();
            await TranslateUserEditView(DefaultLanguage);
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            return await GetEditView(authenticatedUser);
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
            await TranslateUserEditView(DefaultLanguage);
            var authenticatedUser = await SetUser();
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            viewModel.Validate(ModelState);
            if (!ModelState.IsValid)
            {
                return await GetEditView(authenticatedUser);
            }

            // 2. Create a new user if he doesn't exist or update the credentials.
            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            var subject = authenticatedUser.GetSubject();
            if (resourceOwner == null)
            {
                var record = viewModel.ToAddUserParameter();
                record.Login = authenticatedUser.GetSubject();
                record.Claims = authenticatedUser.Claims;
                await _userActions.AddUser(record);
                ViewBag.IsCreated = true;
            }
            else
            {
                await _userActions.UpdateCredentials(subject, viewModel.Password);
                ViewBag.IsUpdated = true;
            }

            return await GetEditView(authenticatedUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaims(Dictionary<string, string> dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException(nameof(dic));
            }
            
            await TranslateUserEditView(DefaultLanguage);
            var authenticatedUser = await SetUser();
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            var claims = new List<Claim>();
            if (dic != null)
            {
                foreach(var kvp in dic)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value));
                }
            }

            await _userActions.UpdateClaims(authenticatedUser.GetSubject(), claims);
            ViewBag.IsUpdated = true;
            return await GetEditView(authenticatedUser);
        }

        /// <summary>
        /// Display the profiles linked to the user account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authenticatedUser = await SetUser();
            var actualScheme = authenticatedUser.Identity.AuthenticationType;
            var profiles = await _profileActions.GetProfiles(authenticatedUser.GetSubject());
            var authenticationSchemes = (await _authenticationSchemeProvider.GetAllSchemesAsync()).Where(a => !string.IsNullOrWhiteSpace(a.DisplayName));
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
            });
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
                var authenticatedUser = await SetUser();
                var externalClaims = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName);                
                var resourceOwner = await _profileActions.Link(authenticatedUser.GetSubject(), externalClaims.GetSubject(), externalClaims.Identity.AuthenticationType, false);
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties());
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }
            catch (ProfileAssignedAnotherAccountException)
            {
                return RedirectToAction("LinkProfileConfirmation");
            }
            catch (Exception)
            {
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties());
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
            var externalClaims = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName);
            if (externalClaims == null ||
                externalClaims.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }

            await SetUser();
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
            var externalClaims = await _authenticationService.GetAuthenticatedUser(this, _authenticateOptions.ExternalCookieName);
            if (externalClaims == null ||
                externalClaims.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await SetUser();
            try
            {
                await _profileActions.Link(authenticatedUser.GetSubject(), externalClaims.GetSubject(), externalClaims.Identity.AuthenticationType, true);
                return RedirectToAction("Profile", "User", new { area = "UserManagement" });
            }
            finally
            {
                await _authenticationService.SignOutAsync(HttpContext, _authenticateOptions.ExternalCookieName, new AuthenticationProperties());
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
            
            var authenticatedUser = await SetUser();
            await _profileActions.Unlink(authenticatedUser.GetSubject(), id);
            return await Profile();
        }

        #endregion

        #region Private methods

        private async Task<IActionResult> GetEditView(ClaimsPrincipal authenticatedUser)
        {
            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            UpdateResourceOwnerViewModel viewModel = null;
            if (resourceOwner == null)
            {
                viewModel = BuildViewModel(authenticatedUser.GetSubject(), authenticatedUser.Claims, false);
                return View("Edit", viewModel);
            }

            viewModel = BuildViewModel(authenticatedUser.GetSubject(), resourceOwner.Claims, true);
            viewModel.IsLocalAccount = true;
            return View("Edit", viewModel);
        }

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
                Core.Constants.StandardTranslationCodes.HashedPassword,
                Core.Constants.StandardTranslationCodes.CreateResourceOwner,
                Core.Constants.StandardTranslationCodes.Credentials,
                Core.Constants.StandardTranslationCodes.RepeatPassword,
                Core.Constants.StandardTranslationCodes.Claims,
                Core.Constants.StandardTranslationCodes.UserIsCreated
            });

            ViewBag.Translations = translations;
        }

        private static UpdateResourceOwnerViewModel BuildViewModel(string subject, IEnumerable<Claim> claims, bool isLocalAccount)
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
            
            return new UpdateResourceOwnerViewModel(subject, editableClaims, notEditableClaims, isLocalAccount);
        }

        #endregion
    }
}
