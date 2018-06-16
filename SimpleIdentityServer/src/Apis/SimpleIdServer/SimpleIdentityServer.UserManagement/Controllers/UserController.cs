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

        /// <summary>
        /// Display the profiles linked to the user account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authenticatedUser = await SetUser();
            var profiles = await _profileActions.Get(authenticatedUser.GetSubject());
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
            
            viewModel.UnlinkedIdentityProviders = authenticationSchemes.Where(a => profiles != null && !profiles.Any(p => p.Issuer == a.Name))
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
