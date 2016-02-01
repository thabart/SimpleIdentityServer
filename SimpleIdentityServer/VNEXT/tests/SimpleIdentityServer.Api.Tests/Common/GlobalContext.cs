using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Testing;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.BackingStoreImplementations;
using Microsoft.Practices.EnterpriseLibrary.Caching.Instrumentation;
using Microsoft.Practices.EnterpriseLibrary.Common.Instrumentation;
using Microsoft.Practices.Unity;
using Moq;
using Owin;
using SimpleIdentityServer.Api.Configuration;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Api.Jwks;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.DataAccess.Fake.Repositories;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.Core.JwtToken;
using System;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Api.UserInfo;
using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.Jwt.Serializer;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class GlobalContext
    {
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public GlobalContext()
        {
            _simpleIdentityServerEventSource = new Mock<ISimpleIdentityServerEventSource>().Object;
            ConfigureContainer();
        }

        public void Init(IGetRateLimitationElementOperation getRateLimitationElementOperation)
        {
            UnityContainer.RegisterInstance(getRateLimitationElementOperation);
        }

        public UnityContainer UnityContainer { get; private set; }

        public ICacheManagerProvider CacheManagerProvider { get; private set; }

        public TestServer CreateServer()
        {
            return GetServer(new HttpConfiguration());
        }

        public TestServer CreateServer(HttpConfiguration configuration)
        {
            return GetServer(configuration);
        }

        public TestServer CreateServer(HttpConfiguration configuration, Action<IAppBuilder> callback)
        {
            return GetServer(configuration, callback);
        }

        private TestServer GetServer(HttpConfiguration configuration, Action<IAppBuilder> callback = null)
        {
            return TestServer.Create(app =>
            {
                SignatureConversions.AddConversions(app);
                RegisterFilterInjector(configuration, UnityContainer);
                configuration.DependencyResolver = new UnityResolver(UnityContainer);
                WebApiConfig.Register(configuration, app, _simpleIdentityServerEventSource);
                if (callback != null)
                {
                    callback(app);
                }
            });
        }

        private static void RegisterFilterInjector(HttpConfiguration config, IUnityContainer container)
        {
            //Register the filter injector
            var providers = config.Services.GetFilterProviders().ToList();
            var defaultprovider = providers.Single(i => i is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(IFilterProvider), defaultprovider);
            config.Services.Add(typeof(IFilterProvider), new UnityFilterProvider(container));
        }

        private void ConfigureContainer()
        {
            UnityContainer = new UnityContainer();
            UnityContainer.RegisterType<ICacheManager, CacheManager>();
            UnityContainer.RegisterType<ISecurityHelper, SecurityHelper>();
            UnityContainer.RegisterType<IClientHelper, ClientHelper>();
            UnityContainer.RegisterType<IConsentHelper, ConsentHelper>();
            UnityContainer.RegisterType<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            UnityContainer.RegisterType<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            
            UnityContainer.RegisterType<IUserInfoActions, UserInfoActions>();
            UnityContainer.RegisterType<IGetJwsPayload, GetJwsPayload>();

            UnityContainer.RegisterType<IClientValidator, ClientValidator>();
            UnityContainer.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            UnityContainer.RegisterType<IScopeValidator, ScopeValidator>();
            UnityContainer.RegisterType<IGrantedTokenValidator, GrantedTokenValidator>();
            UnityContainer
                .RegisterType
                <IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            UnityContainer.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            UnityContainer.RegisterType<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>
                ();
            UnityContainer.RegisterType<IAuthorizationCodeGrantTypeParameterTokenEdpValidator,
                AuthorizationCodeGrantTypeParameterTokenEdpValidator>();

            UnityContainer.RegisterType<IClientRepository, FakeClientRepository>();
            UnityContainer.RegisterType<IScopeRepository, FakeScopeRepository>();
            UnityContainer.RegisterType<IResourceOwnerRepository, FakeResourceOwnerRepository>();
            UnityContainer.RegisterType<IGrantedTokenRepository, FakeGrantedTokenRepository>();
            UnityContainer.RegisterType<IConsentRepository, FakeConsentRepository>();
            UnityContainer.RegisterType<IAuthorizationCodeRepository, FakeAuthorizationCodeRepository>();
            UnityContainer.RegisterType<IJsonWebKeyRepository, FakeJsonWebKeyRepository>();
            UnityContainer.RegisterType<ITranslationRepository, FakeTranslationRepository>();

            UnityContainer.RegisterType<IParameterParserHelper, ParameterParserHelper>();
            UnityContainer.RegisterType<IActionResultFactory, ActionResultFactory>();
            UnityContainer.RegisterType<IHttpClientFactory, HttpClientFactory>();

            UnityContainer
                .RegisterType<IAuthorizationActions, AuthorizationActions>
                ();
            UnityContainer
                .RegisterType<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>
                ();
            UnityContainer.RegisterType<IGetTokenViaImplicitWorkflowOperation, GetTokenViaImplicitWorkflowOperation>();

            UnityContainer.RegisterType<ITokenActions, TokenActions>();
            UnityContainer.RegisterType<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();
            UnityContainer.RegisterType<IGetTokenByAuthorizationCodeGrantTypeAction, GetTokenByAuthorizationCodeGrantTypeAction>();
            UnityContainer.RegisterType<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation, GetAuthorizationCodeAndTokenViaHybridWorkflowOperation>();

            UnityContainer.RegisterType<IConsentActions, ConsentActions>();
            UnityContainer.RegisterType<IConfirmConsentAction, ConfirmConsentAction>();
            UnityContainer.RegisterType<IDisplayConsentAction, DisplayConsentAction>();

            UnityContainer.RegisterType<IAuthenticateActions, AuthenticateActions>();
            UnityContainer.RegisterType<IAuthenticateResourceOwnerAction, AuthenticateResourceOwnerAction>();
            UnityContainer.RegisterType<ILocalUserAuthenticationAction, LocalUserAuthenticationAction>();

            UnityContainer.RegisterType<IRedirectInstructionParser, RedirectInstructionParser>();
            UnityContainer.RegisterType<IActionResultParser, ActionResultParser>();

            UnityContainer.RegisterType<IDiscoveryActions, DiscoveryActions>();
            UnityContainer.RegisterType<ICreateDiscoveryDocumentationAction, CreateDiscoveryDocumentationAction>();

            UnityContainer.RegisterType<IJwksActions, JwksActions>();
            UnityContainer.RegisterType<IRotateJsonWebKeysOperation, RotateJsonWebKeysOperation>();
            UnityContainer.RegisterType<IGetSetOfPublicKeysUsedToValidateJwsAction, GetSetOfPublicKeysUsedToValidateJwsAction>();
            UnityContainer.RegisterType<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
            UnityContainer.RegisterType<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction, GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();


            UnityContainer.RegisterType<IProtector, FakeProtector>();
            UnityContainer.RegisterType<IEncoder, Encoder>();
            UnityContainer.RegisterType<ICertificateStore, CertificateStore>();
            UnityContainer.RegisterType<ICompressor, Compressor>();

            UnityContainer.RegisterType<IProcessAuthorizationRequest, ProcessAuthorizationRequest>();
            UnityContainer.RegisterType<IJwsGenerator, JwsGenerator>();
            UnityContainer.RegisterType<IJweGenerator, JweGenerator>();
            UnityContainer.RegisterType<IJwtParser, JwtParser>();
            UnityContainer.RegisterType<IJweParser, JweParser>();
            UnityContainer.RegisterType<IJweHelper, JweHelper>();
            UnityContainer.RegisterType<IJwtGenerator, JwtGenerator>();
            UnityContainer.RegisterType<IAesEncryptionHelper, AesEncryptionHelper>();
            UnityContainer.RegisterType<IJwsParser, JwsParser>();
            UnityContainer.RegisterType<ICreateJwsSignature, CreateJwsSignature>();
            UnityContainer.RegisterType<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();

            UnityContainer.RegisterType<IClaimsMapping, ClaimsMapping>();

            UnityContainer.RegisterType<IAuthenticateClient, AuthenticateClient>();
            UnityContainer.RegisterType<IAuthenticateHelper, AuthenticateHelper>();
            UnityContainer.RegisterType<IClientSecretBasicAuthentication, ClientSecretBasicAuthentication>();
            UnityContainer.RegisterType<IClientSecretPostAuthentication, ClientSecretPostAuthentication>();
            UnityContainer.RegisterType<IClientAssertionAuthentication, ClientAssertionAuthentication>();

            UnityContainer.RegisterType<IGetTokenByRefreshTokenGrantTypeAction, GetTokenByRefreshTokenGrantTypeAction>();
            UnityContainer.RegisterType<IRefreshTokenGrantTypeParameterValidator, RefreshTokenGrantTypeParameterValidator>();

            UnityContainer.RegisterType<IJsonWebKeyConverter, JsonWebKeyConverter>();
            
            UnityContainer.RegisterType<ICngKeySerializer, CngKeySerializer>();

            UnityContainer.RegisterInstance(_simpleIdentityServerEventSource);

            UnityContainer.RegisterType<ITranslationManager, TranslationManager>();

            var cache = new Cache(new NullBackingStore(),
                new CachingInstrumentationProvider("apiCache", false, false, "simpleIdServer"));
            var instrumentationProvider = new CachingInstrumentationProvider("apiCache", false, false,
                new NoPrefixNameFormatter());
            var cacheManager = new CacheManager(cache, new BackgroundScheduler(new ExpirationTask(cache, instrumentationProvider), new ScavengerTask(
                10, 
                1000, 
                cache, 
                instrumentationProvider), 
                instrumentationProvider), new ExpirationPollTimer(1));

            CacheManagerProvider = new FakeCacheManagerProvider
            {
                CacheManager = cacheManager
            };
            UnityContainer.RegisterInstance(CacheManagerProvider);
        }
    }
}
