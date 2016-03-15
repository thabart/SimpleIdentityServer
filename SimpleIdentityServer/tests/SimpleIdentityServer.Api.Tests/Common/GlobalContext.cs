#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.BackingStoreImplementations;
using Microsoft.Practices.EnterpriseLibrary.Caching.Instrumentation;
using Microsoft.Practices.EnterpriseLibrary.Common.Instrumentation;
using Moq;
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
using SimpleIdentityServer.Core.Api.UserInfo;
using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Serializer;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Models;
using SimpleIdentityServer.DataAccess.Fake.Repositories;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.MiddleWare;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.RateLimitation.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;


namespace SimpleIdentityServer.Api.Tests.Common
{
    public class GlobalContext
    {        
        public class FakeAuthenticationMiddleWareOptions
        {
            public bool IsEnabled { get; set; }

            public ResourceOwner ResourceOwner { get; set; }
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
                    var claims = new List<Claim>();

                    // Add the standard open-id claims.
                    claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, _options.ResourceOwner.Id));
                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.BirthDate))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate, _options.ResourceOwner.BirthDate));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.Email))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email, _options.ResourceOwner.Email));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.FamilyName))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName, _options.ResourceOwner.FamilyName));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.Gender))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender, _options.ResourceOwner.Gender));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.GivenName))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName, _options.ResourceOwner.GivenName));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.Locale))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale, _options.ResourceOwner.Locale));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.MiddleName))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName, _options.ResourceOwner.MiddleName));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.Name))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name, _options.ResourceOwner.Name));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.NickName))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName, _options.ResourceOwner.NickName));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.PhoneNumber))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, _options.ResourceOwner.PhoneNumber));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.Picture))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture, _options.ResourceOwner.Picture));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.PreferredUserName))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName, _options.ResourceOwner.PreferredUserName));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.Profile))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile, _options.ResourceOwner.Profile));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.WebSite))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite, _options.ResourceOwner.WebSite));
                    }

                    if (!string.IsNullOrWhiteSpace(_options.ResourceOwner.ZoneInfo))
                    {
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo, _options.ResourceOwner.ZoneInfo));
                    }

                    var address = _options.ResourceOwner.Address;
                    if (address != null)
                    {
                        var serializedAddress = address.SerializeWithDataContract();
                        claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address, serializedAddress));
                    }

                    claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified, _options.ResourceOwner.EmailVerified.ToString(CultureInfo.InvariantCulture)));
                    claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, _options.ResourceOwner.PhoneNumberVerified.ToString(CultureInfo.InvariantCulture)));
                    claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, _options.ResourceOwner.UpdatedAt.ToString(CultureInfo.InvariantCulture)));
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTime.Now.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture)));

                    var identity = new ClaimsIdentity(claims, "FakeApi");
                    context.Request.HttpContext.User = new ClaimsPrincipal(identity);
                }

                await _next(context);
            }
        }
        
        private class FakeFilterProvider : IFilterProvider, IFilterMetadata
        {
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

            public void OnProvidersExecuted(FilterProviderContext context)
            {
                
            }

            public void OnProvidersExecuting(FilterProviderContext context)
            {
                
            }
        }

        private ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        private IMemoryCache _memoryCache;
        
        public IMemoryCache MemoryCache
        {
            get
            {
                return _memoryCache;
            }
            set
            {
                _memoryCache = value;
            }
        }

        #region Constructor

        public GlobalContext() { }
        
        #endregion
        
        #region Properties
        
        public TestServer TestServer { get; private set;}
        
        public IServiceProvider ServiceProvider { get; private set;}
        
        public FakeDataSource FakeDataSource { get; private set;}
        
        public FakeAuthenticationMiddleWareOptions AuthenticationMiddleWareOptions { get; private set; }

        #endregion

        #region Public methods
        
        public void Init() 
        {
            // Initialize the caching & event source & service collection & data source
            var cache = new Cache(new NullBackingStore(),
                new CachingInstrumentationProvider("apiCache", false, false, "simpleIdServer"));
            var instrumentationProvider = new CachingInstrumentationProvider("apiCache", false, false,
                new NoPrefixNameFormatter());
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _simpleIdentityServerEventSource = new Mock<ISimpleIdentityServerEventSource>().Object;
            var serviceCollection = new ServiceCollection();
            ConfigureServiceCollection(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            FakeDataSource = new FakeDataSource();
            FakeDataSource.Init();
        }
        
        public void CreateServer(Action<IServiceCollection> callback) 
        {
            AuthenticationMiddleWareOptions = new FakeAuthenticationMiddleWareOptions();
            TestServer = TestServer.Create(app => {
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
            serviceCollection.AddTransient<FakeDataSource>(a => FakeDataSource);
            serviceCollection.Configure<RateLimitationOptions>(opt =>
            {
                opt.MemoryCache = _memoryCache;
            });
            serviceCollection.AddMvc();
            serviceCollection.AddLogging();
        }
        
        #endregion
    }
}
