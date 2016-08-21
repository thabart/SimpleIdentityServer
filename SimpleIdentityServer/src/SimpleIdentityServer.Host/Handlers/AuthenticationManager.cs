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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WsFederation;

namespace SimpleIdentityServer.Host.Handlers
{
    public interface IAuthenticationManager
    {
        Task<bool> Initialize(HttpContext httpContext, AuthenticationOptions authenticationOptions);
    }

    internal class AuthenticationManager : IAuthenticationManager
    {
        private readonly ISimpleIdServerConfigurationClientFactory _simpleIdServerConfigurationClientFactory;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly ILogger _logger;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        private readonly IClaimsParser _claimsParser;

        private readonly HtmlEncoder _htmlEncoder;

        #region Constructor

        public AuthenticationManager(
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            IIdentityServerClientFactory identityServerClientFactory,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider,
            HtmlEncoder htmlEncoder,
            IClaimsParser claimsParser)
        {
            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _logger = loggerFactory.CreateLogger("authentication");
            _dataProtectionProvider = dataProtectionProvider;
            _htmlEncoder = htmlEncoder;
            _claimsParser = claimsParser;
        }

        #endregion

        #region Public methods

        public async Task<bool> Initialize(
            HttpContext httpContext,
            AuthenticationOptions authenticationOptions)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (authenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticationOptions));
            }

            var url = httpContext.Request.GetAbsoluteUriWithVirtualPath();
            var grantedToken = await _identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth(authenticationOptions.ClientId, authenticationOptions.ClientSecret)
                .UseClientCredentials("display_configuration")
                .ExecuteAsync(url + "/" + Constants.EndPoints.Token);
            var authProviders = await _simpleIdServerConfigurationClientFactory.GetAuthProviderClient()
                .GetAuthProvidersByResolving(authenticationOptions.ConfigurationUrl, grantedToken.AccessToken);
            foreach (var authProvider in authProviders)
            {
                if (!authProvider.IsEnabled)
                {
                    continue;
                }

                var result = false;
                switch(authProvider.Type)
                {
                    case AuthenticationProviderResponseTypes.OAUTH2:
                        result = await EnableOauth2IdentityProvider(authProvider, httpContext);
                        break;
                    case AuthenticationProviderResponseTypes.OPENID:
                        result = await EnableOpenIdIdentityProvider(authProvider, httpContext);
                        break;
                    case AuthenticationProviderResponseTypes.WSFED:
                        result = await EnableWsFederationAuthentication(authProvider, httpContext);
                        break;                    
                }

                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Enable authentication methods

        private async Task<bool> EnableOauth2IdentityProvider(
            AuthenticationProviderResponse authProvider, 
            HttpContext context)
        {
            var option = ExtractOAuthOptions(authProvider, _dataProtectionProvider);
            if (option == null)
            {
                return false;
            }

            var handler = new OAuthHandler<OAuthOptions>(new HttpClient());
            await handler.InitializeAsync(option, context, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

        private async Task<bool> EnableOpenIdIdentityProvider(
            AuthenticationProviderResponse authProvider,
            HttpContext context)
        {
            HttpClient httpClient;
            var option = ExtractOpenIdConfiguration(authProvider, _dataProtectionProvider, out httpClient);
            if (option == null)
            {
                return false;
            }

            var handler = new OpenIdConnectHandler(httpClient, _htmlEncoder);
            await handler.InitializeAsync(option, context, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

        private async Task<bool> EnableWsFederationAuthentication(
            AuthenticationProviderResponse authProvider,
            HttpContext httpContext)
        {
            var eidOptions = ExtractWsFederationOptions(authProvider);
            if (eidOptions == null)
            {
                return false;
            }

            var handler = new WsFedAuthenticationHandler();
            await handler.InitializeAsync(eidOptions, httpContext, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }
        
        /*                
        private async Task<bool> EnableTwitterAuthentication(List<Option> options, HttpContext httpContext)
        {
            var twitterOptions = ExtractTwitterOptions(options, _dataProtectionProvider);
            if (twitterOptions == null)
            {
                return false;
            }

            var handler = new TwitterHandler(new HttpClient());
            await handler.InitializeAsync(twitterOptions, httpContext, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }
        */

        #endregion

        #region Extract options

        /// <summary>
        /// Extract OAUTH Options
        /// </summary>
        /// <param name="authProvider"></param>
        /// <param name="dataProtectionProvider"></param>
        /// <returns></returns>
        private OAuthOptions ExtractOAuthOptions(
            AuthenticationProviderResponse authProvider,
            IDataProtectionProvider dataProtectionProvider)
        {
            var clientId = authProvider.Options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = authProvider.Options.FirstOrDefault(o => o.Key == "ClientSecret");
            var authorizationEndPoint = authProvider.Options.FirstOrDefault(o => o.Key == "AuthorizationEndpoint");
            var tokenEndpoint = authProvider.Options.FirstOrDefault(o => o.Key == "TokenEndpoint");
            var userInformationEndpoint = authProvider.Options.FirstOrDefault(o => o.Key == "UserInformationEndpoint");
            var code = authProvider.Code;
            var nameSpace = authProvider.Namespace;
            var className = authProvider.ClassName;

            var scopes = authProvider.Options.Where(o => o.Key == "Scope").ToList();
            if (clientId == null || 
                clientSecret == null ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(nameSpace) ||
                string.IsNullOrWhiteSpace(className))
            {
                return null;
            }

            var dataProtector = dataProtectionProvider.CreateProtector(
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Microsoft, "v1");
            
            var result = new OAuthOptions
            {
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                AuthenticationScheme  = authProvider.Name,
                DisplayName = authProvider.Name,
                ClaimsIssuer = authProvider.Name,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                CallbackPath = new PathString(authProvider.CallbackPath),
                AuthorizationEndpoint = authorizationEndPoint.Value,
                TokenEndpoint = tokenEndpoint.Value,
                UserInformationEndpoint = userInformationEndpoint.Value,
                StateDataFormat = new PropertiesDataFormat(dataProtector),                
                Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var endpoint = QueryHelpers.AddQueryString(context.Options.UserInformationEndpoint, "access_token", context.AccessToken);
                        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                        request.Headers.Add("User-Agent", "SimpleIdentityServer");
                        request.Headers.Add("Authorization", "Bearer " + context.AccessToken);
                        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var dic = GetChildrenByReflection(payload);
                        var claims = _claimsParser.Parse(nameSpace, className, code, dic);
                        foreach(var claim in claims)
                        {
                            context.Identity.AddClaim(claim);
                        }
                    }
                }
            };

            result.Scope.Clear();
            if (scopes != null)
            {
                foreach (var scope in scopes.Select(s => s.Value))
                {
                    result.Scope.Add(scope);
                }
            }

            return result;
        }
        
        /// <summary>
        /// Extract OPENID configuration
        /// </summary>
        /// <param name="options"></param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        private OpenIdConnectOptions ExtractOpenIdConfiguration(
            AuthenticationProviderResponse authProvider,
            IDataProtectionProvider dataProtectionProvider,
            out HttpClient httpClient)
        {
            httpClient = null;
            var scopes = authProvider.Options.Where(o => o.Key == "Scope").ToList();
            var clientId = authProvider.Options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = authProvider.Options.FirstOrDefault(o => o.Key == "ClientSecret");
            var wellKnownConfiguration = authProvider.Options.Find(o => o.Key == "WellKnownConfigurationEndPoint");
            var code = authProvider.Code;
            var nameSpace = authProvider.Namespace;
            var className = authProvider.ClassName;
            if (clientId == null || 
                clientSecret == null ||
                wellKnownConfiguration == null ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(nameSpace) ||
                string.IsNullOrWhiteSpace(className))
            {
                return null;
            }

            httpClient = new HttpClient(new HttpClientHandler());
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OpenIdConnect middleware");
            httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10;

            var dataProtector = dataProtectionProvider.CreateProtector(
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Microsoft, "v1");

            var openIdOptions = new OpenIdConnectOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Microsoft,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                DisplayName = Constants.IdentityProviderNames.Microsoft,
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                CallbackPath = new PathString(authProvider.CallbackPath),
                StateDataFormat = new PropertiesDataFormat(dataProtector),
                StringDataFormat = new SecureDataFormat<string>(new StringSerializer(), dataProtector),
                MetadataAddress = wellKnownConfiguration.Value,
                Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = async context =>
                    {
                        string s = "";
                    },
                    OnTicketReceived = async context =>
                    {
                        string s = "";
                    },
                    OnUserInformationReceived = async context =>
                    {
                        string s = "";
                    },
                    OnMessageReceived = async context =>
                    {
                        string s = "";
                    }
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudience = clientId.Value
                },
                ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(wellKnownConfiguration.Value,
                        new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(httpClient) { RequireHttps = true })
            };

            openIdOptions.Scope.Clear();
            if (scopes != null)
            {
                foreach (var scope in scopes.Select(s => s.Value))
                {
                    openIdOptions.Scope.Add(scope);
                }
            }

            return openIdOptions;
        }

        /// <summary>
        /// Extract WS-Federation configuration
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static WsFedAuthenticationOptions ExtractWsFederationOptions(
            AuthenticationProviderResponse authProvider)
        {
            var idEndPoint = authProvider.Options.FirstOrDefault(o => o.Key == "IdPEndpoint");
            var realm = authProvider.Options.FirstOrDefault(o => o.Key == "Realm");
            if (idEndPoint == null ||
                realm == null)
            {
                return null;
            }

            return new WsFedAuthenticationOptions
            {
                IdPEndpoint = idEndPoint.Value,
                Realm = realm.Value,
                AuthenticationScheme = authProvider.Name,
                DisplayName = authProvider.Name,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                AutomaticChallenge = true,
                AutomaticAuthenticate = true,
                RedirectPath = new PathString("/Authenticate/LoginCallback"),
                Events =
                {
                    OnClaimsReceived = xmlNode =>
                    {
                        // Parse and returns the claims
                        return null;
                    }
                }
            };
        }

        /*
        private static TwitterOptions ExtractTwitterOptions(
            List<Option> options,
            IDataProtectionProvider dataProtectionProvider)
        {
            var clientId = options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = options.FirstOrDefault(o => o.Key == "ClientSecret");
            if (clientId == null ||
                clientSecret == null)
            {
                return null;
            }

            var dataProtector = dataProtectionProvider.CreateProtector(
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Twitter, "v1");
            var twitterOptions = new TwitterOptions
            {
                ClaimsIssuer = Constants.IdentityProviderNames.Twitter,
                AuthenticationScheme = Constants.IdentityProviderNames.Twitter,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                ConsumerKey = clientId.Value,
                ConsumerSecret = clientSecret.Value,
                StateDataFormat = new SecureDataFormat<RequestToken>(
                    new RequestTokenSerializer(),
                    dataProtector),
                RetrieveUserDetails = true
            };
            return twitterOptions;
        }
        */
                
        #endregion

        private static Dictionary<string, object> GetChildrenByReflection(JObject jArr)
        {
            var result = new Dictionary<string, object>();
            foreach (KeyValuePair<string, JToken> kvp in jArr)
            {
                var obj = kvp.Value as JObject;
                if (kvp.Value is JObject)
                {
                    result.Add(kvp.Key, GetChildrenByReflection(obj));
                }
                else
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        private class StringSerializer : IDataSerializer<string>
        {
            public string Deserialize(byte[] data)
            {
                return Encoding.UTF8.GetString(data);
            }

            public byte[] Serialize(string model)
            {
                return Encoding.UTF8.GetBytes(model);
            }
        }
    }
}
