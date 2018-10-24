using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleIdentityServer.Authenticate.Basic;
using SimpleIdentityServer.Authenticate.Basic.Controllers;
using SimpleIdentityServer.Authenticate.Basic.ViewModels;
using SimpleIdentityServer.Authenticate.LoginPassword.ViewModels;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.OpenId.Logging;
using SimpleIdServer.Bus;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.LoginPassword.Controllers
{
    [Area(Constants.AMR)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IProfileActions profileActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            IOpenIdEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUserActions userActions,
            IPayloadSerializer payloadSerializer,
            IConfigurationService configurationService,
            IAuthenticateHelper authenticateHelper,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            BasicAuthenticateOptions basicAuthenticateOptions) : base(authenticateActions, profileActions, dataProtectionProvider, encoder,
                translationManager, simpleIdentityServerEventSource, urlHelperFactory, actionContextAccessor, eventPublisher,
                authenticationService, authenticationSchemeProvider, userActions, payloadSerializer, configurationService,
                authenticateHelper, twoFactorAuthenticationHandler, basicAuthenticateOptions)
        {
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
        }

        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpPost]
        public async Task<ActionResult> LocalLogin(LocalAuthenticationViewModel authorizeViewModel)
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser != null &&
                authenticatedUser.Identity != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException(nameof(authorizeViewModel));
            }

            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View("Index", viewModel);
            }

            try
            {
                var resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(authorizeViewModel.Login, authorizeViewModel.Password, new[] { Constants.AMR });                if (resourceOwner == null)
                {
                    throw new IdentityServerAuthenticationException("the resource owner credentials are not correct");
                }

                var claims = resourceOwner.Claims;
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
                var subject = claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
                if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
                {
                    await SetLocalCookie(claims, Guid.NewGuid().ToString());
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                    return RedirectToAction("Index", "User", new { area = "UserManagement" });
                }

                // 2.1 Store temporary information in cookie
                await SetTwoFactorCookie(claims);
                // 2.2. Send confirmation code
                try
                {
                    var code = await _authenticateActions.GenerateAndSendCode(subject);
                    _simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode");
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode");
                }
                catch(Exception)
                {
                    throw new Exception("Two factor authenticator is not properly configured");
                }
            }
            catch (Exception exception)
            {
                _simpleIdentityServerEventSource.Failure(exception.Message);
                await TranslateView(DefaultLanguage);
                ModelState.AddModelError("invalid_credentials", exception.Message);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View("Index", viewModel);
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> LocalLoginOpenId(OpenidLocalAuthenticationViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                throw new ArgumentNullException(nameof(viewModel.Code));
            }

            await SetUser();
            var uiLocales = DefaultLanguage;
            try
            {
                // 1. Decrypt the request
                var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);
                
                // 2. Retrieve the default language
                uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;
                
                // 3. Check the state of the view model
                if (!ModelState.IsValid)
                {
                    await TranslateView(uiLocales);
                    await SetIdProviders(viewModel);
                    return View("OpenId", viewModel);
                }

                // 4. Local authentication
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateActions.LocalOpenIdUserAuthentication(new LocalAuthenticationParameter
                    {
                        UserName = viewModel.Login,
                        Password = viewModel.Password
                    },
                    request.ToParameter(),
                    viewModel.Code, issuerName);
                var subject = actionResult.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;

                // 5. Two factor authentication.
                if (!string.IsNullOrWhiteSpace(actionResult.TwoFactor))
                {
					try
					{
						await SetTwoFactorCookie(actionResult.Claims);
						var code = await _authenticateActions.GenerateAndSendCode(subject);
						_simpleIdentityServerEventSource.GetConfirmationCode(code);
						return RedirectToAction("SendCode", new { code = viewModel.Code });
					}
					catch(ClaimRequiredException)
					{
						return RedirectToAction("SendCode", new { code = viewModel.Code });
					}
                    catch(Exception)
                    {
                        ModelState.AddModelError("invalid_credentials", "Two factor authenticator is not properly configured");
                    }
                }
                else
                {
                    // 6. Authenticate the user by adding a cookie
                    await SetLocalCookie(actionResult.Claims, request.SessionId);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                    // 7. Redirect the user agent
                    var result = this.CreateRedirectionFromActionResult(actionResult.ActionResult,
                        request);
                    if (result != null)
                    {
                        LogAuthenticateUser(actionResult.ActionResult, request.ProcessId);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            await TranslateView(uiLocales);
            await SetIdProviders(viewModel);
            return View("OpenId", viewModel);
        }
    }
}
