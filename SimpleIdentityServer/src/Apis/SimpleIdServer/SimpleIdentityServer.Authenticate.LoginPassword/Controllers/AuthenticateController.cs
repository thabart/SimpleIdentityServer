using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SimpleBus.Core;
using SimpleIdentityServer.Authenticate.Basic;
using SimpleIdentityServer.Authenticate.Basic.Controllers;
using SimpleIdentityServer.Authenticate.Basic.ViewModels;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Logging;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.LoginPassword.Controllers
{
    [Area("AuthLoginPassword")]
    public class AuthenticateController : BaseAuthenticateController
    {
        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IProfileActions profileActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            ITranslationManager translationManager,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
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
            BasicAuthenticateOptions basicAuthenticateOptions,
            AuthenticateOptions authenticateOptions) : base(authenticateActions, profileActions, dataProtectionProvider, encoder,
                translationManager, simpleIdentityServerEventSource, urlHelperFactory, actionContextAccessor, eventPublisher,
                authenticationService, authenticationSchemeProvider, userActions, payloadSerializer, configurationService,
                authenticateHelper, twoFactorAuthenticationHandler, basicAuthenticateOptions, authenticateOptions)
        {

        }

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
    }
}
