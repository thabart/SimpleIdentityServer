using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleBus.Core;
using SimpleIdentityServer.Authenticate.Basic.Controllers;
using SimpleIdentityServer.Authenticate.Basic.ViewModels;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.ViewModels;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Controllers
{
    [Area(Constants.AMR)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private const string _passwordLessCookieName = "SimpleIdentityServer-PasswordLess";
        private readonly ISmsAuthenticationOperation _smsAuthenticationOperation;
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

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
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            ISmsAuthenticationOperation smsAuthenticationOperation,
            IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation,
            SmsAuthenticationOptions basicAuthenticateOptions,
            AuthenticateOptions authenticateOptions) : base(authenticateActions, profileActions, dataProtectionProvider, encoder,
                translationManager, simpleIdentityServerEventSource, urlHelperFactory, actionContextAccessor, eventPublisher,
                authenticationService, authenticationSchemeProvider, userActions, payloadSerializer, configurationService,
                authenticateHelper, twoFactorAuthenticationHandler, basicAuthenticateOptions, authenticateOptions)
        {
            _smsAuthenticationOperation = smsAuthenticationOperation;
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser == null ||
                authenticatedUser.Identity == null ||
                !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpPost]
        public async Task<IActionResult> LocalLogin(LocalAuthenticationViewModel localAuthenticationViewModel)
        {
            var authenticatedUser = await SetUser();
            if (authenticatedUser != null &&
                authenticatedUser.Identity != null &&
                authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            if (localAuthenticationViewModel == null)
            {
                throw new ArgumentNullException(nameof(localAuthenticationViewModel));
            }

            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation.Execute(localAuthenticationViewModel.PhoneNumber);
                }
                catch (IdentityServerAuthenticationException ex)
                {
                    _simpleIdentityServerEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    var claims = resourceOwner.Claims;
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                        DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(claims);
                    try
                    {
                        var code = await _generateAndSendSmsCodeOperation.Execute(localAuthenticationViewModel.PhoneNumber, claims.GetSubject());
                        _simpleIdentityServerEventSource.GetConfirmationCode(code);
                        return RedirectToAction("ConfirmCode");
                    }
                    catch (Exception ex)
                    {
                        _simpleIdentityServerEventSource.Failure(ex.Message);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel);
            await TranslateView(DefaultLanguage);
            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmCode(string code)
        {
            var user = await SetUser();
            if (user != null &&
                user.Identity != null &&
                user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _passwordLessCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, "SMS authentication cannot be performed");
            }

            await TranslateView(DefaultLanguage);
            return View(new ConfirmCodeViewModel
            {
                Code = code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCode(ConfirmCodeViewModel confirmCodeViewModel)
        {
            if (confirmCodeViewModel == null)
            {
                throw new ArgumentNullException(nameof(confirmCodeViewModel));
            }

            var user = await SetUser();
            if (user != null &&
                user.Identity != null &&
                user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, _passwordLessCookieName);
            if (authenticatedUser == null || authenticatedUser.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(Core.Errors.ErrorCodes.UnhandledExceptionCode, "SMS authentication cannot be performed");
            }

            var subject = authenticatedUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value;
            var phoneNumber = authenticatedUser.Claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
            if (confirmCodeViewModel.Action == "resend") // Resend the confirmation code.
            {
                var code = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value, subject);
                _simpleIdentityServerEventSource.GetConfirmationCode(code);
                await TranslateView(DefaultLanguage);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            if (!await _authenticateActions.ValidateCode(confirmCodeViewModel.ConfirmationCode)) // Check the confirmation code.
            {
                ModelState.AddModelError("message_error", "Confirmation code is not valid");
                await TranslateView(DefaultLanguage);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            await _authenticationService.SignOutAsync(HttpContext, _passwordLessCookieName, new AuthenticationProperties());
            var resourceOwner = await _userActions.GetUser(authenticatedUser);
            if (!string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication)) // Execute TWO Factor authentication
            {
                try
                {
                    await SetTwoFactorCookie(authenticatedUser.Claims);
                    var code = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value, subject);
                    _simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch(Exception)
                {
                    ModelState.AddModelError("message_error", "Two factor authenticator is not properly configured");
                    await TranslateView(DefaultLanguage);
                    return View("ConfirmCode", confirmCodeViewModel);
                }
            }

            _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
            if (!string.IsNullOrWhiteSpace(confirmCodeViewModel.Code)) // Execute OPENID workflow
            {
                var request = _dataProtector.Unprotect<AuthorizationRequest>(confirmCodeViewModel.Code);
                await SetLocalCookie(authenticatedUser.Claims, request.SessionId);
                var actionResult = await _authenticateHelper.ProcessRedirection(request.ToParameter(), confirmCodeViewModel.Code, subject, authenticatedUser.Claims.ToList());
                var result = this.CreateRedirectionFromActionResult(actionResult, request);
                if (result != null)
                {
                    LogAuthenticateUser(actionResult, request.ProcessId);
                    return result;
                }
            }

            await SetLocalCookie(authenticatedUser.Claims, Guid.NewGuid().ToString()); // Authenticate the resource owner
            return RedirectToAction("Index", "User", new { area = "UserManagement" });
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
            // 1. Decrypt the request
            var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);
            // 2. Retrieve the default language
            uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;
            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation.Execute(viewModel.PhoneNumber);
                }
                catch (IdentityServerAuthenticationException ex)
                {
                    _simpleIdentityServerEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    var claims = resourceOwner.Claims;
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                        DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(claims);
                    try
                    {
                        var code = await _generateAndSendSmsCodeOperation.Execute(viewModel.PhoneNumber, claims.GetSubject());
                        _simpleIdentityServerEventSource.GetConfirmationCode(code);
                        return RedirectToAction("ConfirmCode", new { code = viewModel.Code });
                    }
                    catch (Exception ex)
                    {
                        _simpleIdentityServerEventSource.Failure(ex.Message);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            await TranslateView(uiLocales);
            await SetIdProviders(viewModel);
            return View("OpenId", viewModel);
        }

        private async Task SetPasswordLessCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, _passwordLessCookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext, _passwordLessCookieName, principal, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                IsPersistent = false
            });
        }
    }
}
