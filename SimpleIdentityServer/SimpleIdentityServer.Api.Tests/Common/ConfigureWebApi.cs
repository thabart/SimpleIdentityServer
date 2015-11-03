using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;

using Microsoft.Owin.Testing;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.Unity;

using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.Api.Controllers.Api;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Repositories;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Api.Parsers;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class ConfigureWebApi
    {
        private readonly UnityContainer _container;

        public ConfigureWebApi()
        {
            _container = new UnityContainer();
            _container.RegisterType<ICacheManager, CacheManager>();
            _container.RegisterType<ISecurityHelper, SecurityHelper>();
            _container.RegisterType<ITokenHelper, TokenHelper>();

            _container.RegisterType<IClientValidator, ClientValidator>();
            _container.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            _container.RegisterType<IScopeValidator, ScopeValidator>();
            _container
                .RegisterType
                <IAuthorizationCodeGrantTypeParameterValidator, AuthorizationCodeGrantTypeParameterValidator>();
            _container.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            _container.RegisterType<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>
                ();

            _container.RegisterType<IClientRepository, FakeClientRepository>();
            _container.RegisterType<IScopeRepository, FakeScopeRepository>();
            _container.RegisterType<IResourceOwnerRepository, FakeResourceOwnerRepository>();
            _container.RegisterType<IGrantedTokenRepository, FakeGrantedTokenRepository>();
            _container.RegisterType<IConsentRepository, FakeConsentRepository>();
            _container.RegisterType<IAuthorizationCodeRepository, FakeAuthorizationCodeRepository>();

            _container.RegisterType<IParameterParserHelper, ParameterParserHelper>();
            _container.RegisterType<IActionResultFactory, ActionResultFactory>();

            _container
                .RegisterType<IAuthorizationActions, AuthorizationActions>
                ();
            _container
                .RegisterType<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>
                ();
            _container.RegisterType<IGetTokenViaImplicitWorkflowOperation, GetTokenViaImplicitWorkflowOperation>();

            _container.RegisterType<ITokenActions, TokenActions>();
            _container.RegisterType<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();

            _container.RegisterType<IConsentActions, ConsentActions>();
            _container.RegisterType<IConfirmConsentAction, ConfirmConsentAction>();
            _container.RegisterType<IDisplayConsentAction, DisplayConsentAction>();

            _container.RegisterType<IAuthenticateActions, AuthenticateActions>();
            _container.RegisterType<IAuthenticateResourceOwnerAction, AuthenticateResourceOwnerAction>();
            _container.RegisterType<ILocalUserAuthenticationAction, LocalUserAuthenticationAction>();

            _container.RegisterType<IRedirectInstructionParser, RedirectInstructionParser>();
            _container.RegisterType<IActionResultParser, ActionResultParser>();

            _container.RegisterType<IProtector, Protector>();
            _container.RegisterType<IEncoder, Encoder>();
            _container.RegisterType<ICertificateStore, CertificateStore>();
            _container.RegisterType<ICompressor, Compressor>();

            var t = _container.Resolve<AuthorizationController>();

            FakeDataSource.Instance().Init();
        }

        public UnityContainer Container
        {
            get { return _container; }
        }

        public IGetRateLimitationElementOperation GetRateLimitationElementOperation { get; private set; }

        public TestServer CreateServer()
        {
            return GetServer(new HttpConfiguration());
        }

        public TestServer CreateServer(HttpConfiguration configuration)
        {
            return GetServer(configuration);
        }

        private static void RegisterFilterInjector(HttpConfiguration config, IUnityContainer container)
        {
            //Register the filter injector
            var providers = config.Services.GetFilterProviders().ToList();
            var defaultprovider = providers.Single(i => i is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(IFilterProvider), defaultprovider);
            config.Services.Add(typeof(IFilterProvider), new UnityFilterProvider(container));
        }

        private TestServer GetServer(HttpConfiguration configuration)
        {
            return TestServer.Create(app =>
            {
                RegisterFilterInjector(configuration, _container);
                configuration.DependencyResolver = new UnityResolver(_container);
                WebApiConfig.Register(configuration, app);
            });
        }
    }
}
