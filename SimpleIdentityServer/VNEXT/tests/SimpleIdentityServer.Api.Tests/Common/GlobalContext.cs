using System.Web.Http;
using System.Web.Http.Filters;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.BackingStoreImplementations;
using Microsoft.Practices.EnterpriseLibrary.Caching.Instrumentation;
using Microsoft.Practices.EnterpriseLibrary.Common.Instrumentation;
using Moq;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Builder;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Claims;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using SimpleIdentityServer.Host.MiddleWare;
using SimpleIdentityServer.RateLimitation.Attributes;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class GlobalContext
    {
        /*
        private class FakeAuthorizationFilterAttribute : Microsoft.AspNet.Mvc.Filters.IAuthorizationFilter, IFilterMetadata
        {
            static FakeAuthorizationFilterAttribute()
            {
                TestUserId = "TestDomain\\TestUser";
            }

            public bool AllowMultiple { get; private set; }

            public static string TestUserId { get; set; }

            public void OnAuthorization(Microsoft.AspNet.Mvc.Filters.AuthorizationContext context)
            {
                
            }
        }
        */
        
        public class FakeAuthenticationMiddleWareOptions
        {
            public bool IsEnabled { get; set; }

            public string Subject { get; set; }
        }

        private class FakeAuthenticationMiddleWare 
        {            
            private readonly RequestDelegate _next;

            private readonly FakeAuthenticationMiddleWareOptions _options;

            public FakeAuthenticationMiddleWare(
                RequestDelegate next,
                FakeAuthenticationMiddleWareOptions options) 
            {
                _next = next;
                _options = options;
            }
            
            public async Task Invoke(HttpContext context) 
            {
                if (_options.IsEnabled)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("sub", _options.Subject)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "default");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    context.Request.HttpContext.User = claimsPrincipal;
                }

                await _next(context);
            }
        }
        
        private class FakeFilterProvider : Microsoft.AspNet.Mvc.Filters.IFilterProvider, IFilterMetadata
        {
            private readonly ActionDescriptorFilterProvider _defaultProvider = new ActionDescriptorFilterProvider();

            private readonly IServiceCollection _serviceCollection;

            public FakeFilterProvider(IServiceCollection serviceCollection)
            {
                _serviceCollection = serviceCollection;
            }

            public int Order
            {
                get
                {
                    return 1;
                }
            }

            public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
            {
                var attributes = _defaultProvider.GetFilters(configuration, actionDescriptor);
                foreach (var attr in attributes)
                {
                    _serviceCollection.AddInstance(attr.Instance.GetType(), attr.Instance);
                    // _container.BuildUp(attr.Instance.GetType(), attr.Instance);
                }

                return attributes;
            }

            public void OnProvidersExecuted(FilterProviderContext context)
            {
                
            }

            public void OnProvidersExecuting(FilterProviderContext context)
            {
                
            }
        }

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        
        private readonly ICacheManagerProvider _cacheManagerProvider;

        #region Constructor

        public GlobalContext()
        {
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

            _cacheManagerProvider = new FakeCacheManagerProvider
            {
                CacheManager = cacheManager
            };
            _simpleIdentityServerEventSource = new Mock<ISimpleIdentityServerEventSource>().Object;
            var serviceCollection = new ServiceCollection();
            ConfigureServiceCollection(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
        
        #endregion
        
        #region Properties
        
        public TestServer TestServer { get; private set;}
        
        public IServiceProvider ServiceProvider { get; private set;}
        
        public FakeAuthenticationMiddleWareOptions AuthenticationMiddleWareOptions { get; private set; }

        #endregion

        #region Public methods


        public void CreateServer(Action<IServiceCollection> callback) 
        {
            AuthenticationMiddleWareOptions = new FakeAuthenticationMiddleWareOptions();
            TestServer = TestServer.Create(app => {
                /*             
                app.UseCookieAuthentication(opts => {
                    opts.AuthenticationScheme = "SimpleIdentityServerAuthentication";
                    opts.AutomaticChallenge = true;
                });
                */

                app.UseSimpleIdentityServerExceptionHandler(new ExceptionHandlerMiddlewareOptions
                {
                    SimpleIdentityServerEventSource = SimpleIdentityServerEventSource.Log
                });
                app.UseMiddleware<FakeAuthenticationMiddleWare>(AuthenticationMiddleWareOptions);
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action=Index}/{id?}");
                });
            }, services => {
                ConfigureServiceCollection(services);
                if (callback != null) {
                    callback(services);
                }
            });
        }
        
        #endregion
        
        #region Private methods

        private void ConfigureServiceCollection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICacheManager, CacheManager>();
            serviceCollection.AddTransient<ISecurityHelper, SecurityHelper>();
            serviceCollection.AddTransient<IClientHelper, ClientHelper>();
            serviceCollection.AddTransient<IConsentHelper, ConsentHelper>();
            serviceCollection.AddTransient<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            serviceCollection.AddTransient<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();            
            serviceCollection.AddTransient<IUserInfoActions, UserInfoActions>();
            serviceCollection.AddTransient<IGetJwsPayload, GetJwsPayload>();
            serviceCollection.AddTransient<IClientValidator, ClientValidator>();
            serviceCollection.AddTransient<IResourceOwnerValidator, ResourceOwnerValidator>();
            serviceCollection.AddTransient<IScopeValidator, ScopeValidator>();
            serviceCollection.AddTransient<IGrantedTokenValidator, GrantedTokenValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            serviceCollection.AddTransient<IResourceOwnerValidator, ResourceOwnerValidator>();
            serviceCollection.AddTransient<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterTokenEdpValidator,AuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            serviceCollection.AddTransient<IClientRepository, FakeClientRepository>();
            serviceCollection.AddTransient<IScopeRepository, FakeScopeRepository>();
            serviceCollection.AddTransient<IResourceOwnerRepository, FakeResourceOwnerRepository>();
            serviceCollection.AddTransient<IGrantedTokenRepository, FakeGrantedTokenRepository>();
            serviceCollection.AddTransient<IConsentRepository, FakeConsentRepository>();
            serviceCollection.AddTransient<IAuthorizationCodeRepository, FakeAuthorizationCodeRepository>();
            serviceCollection.AddTransient<IJsonWebKeyRepository, FakeJsonWebKeyRepository>();
            serviceCollection.AddTransient<ITranslationRepository, FakeTranslationRepository>();
            serviceCollection.AddTransient<IParameterParserHelper, ParameterParserHelper>();
            serviceCollection.AddTransient<IActionResultFactory, ActionResultFactory>();
            serviceCollection.AddTransient<IHttpClientFactory, Core.Factories.HttpClientFactory>();
            serviceCollection.AddTransient<IAuthorizationActions, AuthorizationActions>();
            serviceCollection.AddTransient<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>();
            serviceCollection.AddTransient<IGetTokenViaImplicitWorkflowOperation, GetTokenViaImplicitWorkflowOperation>();
            serviceCollection.AddTransient<ITokenActions, TokenActions>();
            serviceCollection.AddTransient<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();
            serviceCollection.AddTransient<IGetTokenByAuthorizationCodeGrantTypeAction, GetTokenByAuthorizationCodeGrantTypeAction>();
            serviceCollection.AddTransient<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation, GetAuthorizationCodeAndTokenViaHybridWorkflowOperation>();
            serviceCollection.AddTransient<IConsentActions, ConsentActions>();
            serviceCollection.AddTransient<IConfirmConsentAction, ConfirmConsentAction>();
            serviceCollection.AddTransient<IDisplayConsentAction, DisplayConsentAction>();
            serviceCollection.AddTransient<IAuthenticateActions, AuthenticateActions>();
            serviceCollection.AddTransient<IAuthenticateResourceOwnerAction, AuthenticateResourceOwnerAction>();
            serviceCollection.AddTransient<ILocalUserAuthenticationAction, LocalUserAuthenticationAction>();
            serviceCollection.AddTransient<IRedirectInstructionParser, RedirectInstructionParser>();
            serviceCollection.AddTransient<IActionResultParser, ActionResultParser>();
            serviceCollection.AddTransient<IDiscoveryActions, DiscoveryActions>();
            serviceCollection.AddTransient<ICreateDiscoveryDocumentationAction, CreateDiscoveryDocumentationAction>();
            serviceCollection.AddTransient<IJwksActions, JwksActions>();
            serviceCollection.AddTransient<IRotateJsonWebKeysOperation, RotateJsonWebKeysOperation>();
            serviceCollection.AddTransient<IGetSetOfPublicKeysUsedToValidateJwsAction, GetSetOfPublicKeysUsedToValidateJwsAction>();
            serviceCollection.AddTransient<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
            serviceCollection.AddTransient<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction, GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();
            serviceCollection.AddTransient<IProtector, FakeProtector>();
            serviceCollection.AddTransient<IEncoder, Encoder>();
            serviceCollection.AddTransient<ICertificateStore, CertificateStore>();
            serviceCollection.AddTransient<ICompressor, Compressor>();
            serviceCollection.AddTransient<IProcessAuthorizationRequest, ProcessAuthorizationRequest>();
            serviceCollection.AddTransient<IJwsGenerator, JwsGenerator>();
            serviceCollection.AddTransient<IJweGenerator, JweGenerator>();
            serviceCollection.AddTransient<IJwtParser, JwtParser>();
            serviceCollection.AddTransient<IJweParser, JweParser>();
            serviceCollection.AddTransient<IJweHelper, JweHelper>();
            serviceCollection.AddTransient<IJwtGenerator, JwtGenerator>();
            serviceCollection.AddTransient<IAesEncryptionHelper, AesEncryptionHelper>();
            serviceCollection.AddTransient<IJwsParser, JwsParser>();
            serviceCollection.AddTransient<ICreateJwsSignature, CreateJwsSignature>();
            serviceCollection.AddTransient<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();
            serviceCollection.AddTransient<IClaimsMapping, ClaimsMapping>();
            serviceCollection.AddTransient<IAuthenticateClient, AuthenticateClient>();
            serviceCollection.AddTransient<IAuthenticateHelper, AuthenticateHelper>();
            serviceCollection.AddTransient<IClientSecretBasicAuthentication, ClientSecretBasicAuthentication>();
            serviceCollection.AddTransient<IClientSecretPostAuthentication, ClientSecretPostAuthentication>();
            serviceCollection.AddTransient<IClientAssertionAuthentication, ClientAssertionAuthentication>();
            serviceCollection.AddTransient<IGetTokenByRefreshTokenGrantTypeAction, GetTokenByRefreshTokenGrantTypeAction>();
            serviceCollection.AddTransient<IRefreshTokenGrantTypeParameterValidator, RefreshTokenGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IJsonWebKeyConverter, JsonWebKeyConverter>();            
            serviceCollection.AddTransient<ICngKeySerializer, CngKeySerializer>();
            serviceCollection.AddInstance(_simpleIdentityServerEventSource);
            serviceCollection.AddTransient<ITranslationManager, TranslationManager>();
            serviceCollection.AddInstance(_cacheManagerProvider);
            // var filter = new FakeFilterProvider(serviceCollection);
            serviceCollection.AddMvc(/*opt => {
                opt.Filters.Add(filter);
            }*/);
            serviceCollection.AddScoped<RateLimitationFilter>();
            serviceCollection.AddLogging();
        }
        
        #endregion
    }
}
