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
using Microsoft.AspNetCore.Authentication.Twitter;
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

                if (authProvider.Type == AuthenticationProviderResponseTypes.OAUTH2)
                {
                    if (await EnableOauth2IdentityProvider(authProvider, httpContext))
                    {
                        return true;
                    }
                }
                else if (authProvider.Type == AuthenticationProviderResponseTypes.OPENID)
                {
                    if (await EnableOpenIdIdentityProvider(authProvider, httpContext))
                    {
                        return true;
                    }
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

        private async Task<bool> EnableAdfsAuthentication(List<Option> options, HttpContext httpContext)
        {
            var adfsOptions = ExtractAdfsOptions(options, _dataProtectionProvider);
            if (adfsOptions == null)
            {
                return false;
            }

            var handler = new OAuthHandler<OAuthOptions>(new HttpClient());
            await handler.InitializeAsync(adfsOptions, httpContext, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

        private async Task<bool> EnableEidAuthentication(List<Option> options, HttpContext httpContext)
        {
            var eidOptions = ExtractEidOptions(options);
            if (eidOptions == null)
            {
                return false;
            }

            var handler = new WsFedAuthenticationHandler();
            await handler.InitializeAsync(eidOptions, httpContext, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

        private async Task<bool> EnableGoogleAuthentication(List<Option> options, HttpContext httpContext)
        {
            var googleOptions = ExtractGoogleOptions(options, _dataProtectionProvider);
            if (googleOptions == null)
            {
                return false;
            }

            var handler = new GoogleHandler(new HttpClient());
            await handler.InitializeAsync(googleOptions, httpContext, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

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

        public async Task<bool> EnableGithubAuthentication(List<Option> options, HttpContext httpContext)
        {
            HttpClient httpClient = null;
            var githubOptions = ExtractGithubOptions(options, _dataProtectionProvider, out httpClient);
            if (githubOptions == null)
            {
                return false;
            }
            
            var handler = new GitHubAuthenticationHandler(httpClient);
            await handler.InitializeAsync(githubOptions, httpContext, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

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
                        var response = await context.Backchannel.GetAsync(endpoint, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var dic = payload.ToObject<Dictionary<string, object>>();
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

        public static GoogleOptions ExtractGoogleOptions(
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
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Google, "v1");
            var googleOptions = new GoogleOptions
            {
                ClaimsIssuer = Constants.IdentityProviderNames.Google,
                AuthenticationScheme = Constants.IdentityProviderNames.Google,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                StateDataFormat = new PropertiesDataFormat(dataProtector)
            };

            return googleOptions;
        }

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

        public static GitHubAuthenticationOptions ExtractGithubOptions(
            List<Option> options,
            IDataProtectionProvider dataProtectionProvider,
            out HttpClient httpClient)
        {
            httpClient = null;
            var clientId = options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = options.FirstOrDefault(o => o.Key == "ClientSecret");
            if (clientId == null ||
                clientSecret == null)
            {
                return null;
            }

            var githubOptions = new GitHubAuthenticationOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Github,
                DisplayName = Constants.IdentityProviderNames.Github,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                ClaimsIssuer = Constants.IdentityProviderNames.Github
            };
            EnrichOauthOptions(githubOptions, dataProtectionProvider, out httpClient);
            return githubOptions;
        }
                
        private static OAuthOptions ExtractAdfsOptions(
            List<Option> options, 
            IDataProtectionProvider dataProtectionProvider)
        {
            var clientId = options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = options.FirstOrDefault(o => o.Key == "ClientSecret");
            var relyingParty = options.FirstOrDefault(o => o.Key == "RelyingParty");
            var claimsIssuer = options.FirstOrDefault(o => o.Key == "ClaimsIssuer");
            var authorizationEndPoint = options.FirstOrDefault(o => o.Key == "AdfsAuthorizationEndPoint");
            var tokenEndPoint = options.FirstOrDefault(o => o.Key == "AdfsTokenEndPoint");
            if (clientId == null 
                || clientSecret == null
                || relyingParty == null
                || claimsIssuer == null
                || authorizationEndPoint == null 
                || tokenEndPoint == null)
            {
                return null;
            }

            var dataProtector = dataProtectionProvider.CreateProtector(
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Adfs, "v1");

            var adfsOptions = new OAuthOptions
            {
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                AuthenticationScheme = Constants.IdentityProviderNames.Adfs,
                DisplayName = Constants.IdentityProviderNames.Adfs,
                CallbackPath = new PathString("/oauth-callback"),
                Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = async context =>
                    {
                        var parameter = new Dictionary<string, string>
                        {
                            ["resource"] = relyingParty.Value
                        };
                        var query = QueryHelpers.AddQueryString(context.RedirectUri, parameter);
                        context.Response.Redirect(query);
                    }
                },
                ClaimsIssuer = claimsIssuer.Value,
                AuthorizationEndpoint = authorizationEndPoint.Value,
                TokenEndpoint = tokenEndPoint.Value,
                StateDataFormat = new PropertiesDataFormat(dataProtector)
            };
            return adfsOptions;
        }

        private static WsFedAuthenticationOptions ExtractEidOptions(List<Option> options)
        {
            var idEndPoint = options.FirstOrDefault(o => o.Key == "IdPEndpoint");
            var realm = options.FirstOrDefault(o => o.Key == "Realm");
            if (idEndPoint == null ||
                realm == null)
            {
                return null;
            }

            return new WsFedAuthenticationOptions
            {
                IdPEndpoint = idEndPoint.Value,
                Realm = realm.Value,
                AuthenticationScheme = Constants.IdentityProviderNames.Eid,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                AutomaticChallenge = true,
                AutomaticAuthenticate = true,
                DisplayName = "Belg. eid card",
                RedirectPath = new PathString("/Authenticate/LoginCallback")
            };
        }

        private static void EnrichOpenidOptions(
            OpenIdConnectOptions openIdConnectOptions,
            IDataProtectionProvider dataProtectionProvider,
            out HttpClient backChannel)
        {
            if (openIdConnectOptions.StateDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(AuthenticationManager).FullName,
                    typeof(string).FullName,
                    openIdConnectOptions.AuthenticationScheme,
                    "v1");

                openIdConnectOptions.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            if (openIdConnectOptions.StringDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(AuthenticationManager).FullName,
                    typeof(string).FullName,
                    openIdConnectOptions.AuthenticationScheme,
                    "v1");

                openIdConnectOptions.StringDataFormat = new SecureDataFormat<string>(new StringSerializer(), dataProtector);
            }

            if (openIdConnectOptions.Events == null)
            {
                openIdConnectOptions.Events = new OpenIdConnectEvents();
            }
            if (string.IsNullOrEmpty(openIdConnectOptions.TokenValidationParameters.ValidAudience) && !string.IsNullOrEmpty(openIdConnectOptions.ClientId))
            {
                openIdConnectOptions.TokenValidationParameters.ValidAudience = openIdConnectOptions.ClientId;
            }

            backChannel = new HttpClient(openIdConnectOptions.BackchannelHttpHandler ?? new HttpClientHandler());
            backChannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OpenIdConnect middleware");
            backChannel.Timeout = openIdConnectOptions.BackchannelTimeout;
            backChannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB

            if (openIdConnectOptions.ConfigurationManager == null)
            {
                if (openIdConnectOptions.Configuration != null)
                {
                    openIdConnectOptions.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(openIdConnectOptions.Configuration);
                }
                else if (!(string.IsNullOrEmpty(openIdConnectOptions.MetadataAddress) && string.IsNullOrEmpty(openIdConnectOptions.Authority)))
                {
                    if (string.IsNullOrEmpty(openIdConnectOptions.MetadataAddress) && !string.IsNullOrEmpty(openIdConnectOptions.Authority))
                    {
                        openIdConnectOptions.MetadataAddress = openIdConnectOptions.Authority;
                        if (!openIdConnectOptions.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            openIdConnectOptions.MetadataAddress += "/";
                        }

                        openIdConnectOptions.MetadataAddress += ".well-known/openid-configuration";
                    }

                    if (openIdConnectOptions.RequireHttpsMetadata && !openIdConnectOptions.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
                    }

                    openIdConnectOptions.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConnectOptions.MetadataAddress, new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(backChannel) { RequireHttps = openIdConnectOptions.RequireHttpsMetadata });
                }
            }
        }

        private static void EnrichOauthOptions(
            OAuthOptions oauthOptions,
            IDataProtectionProvider dataProtectionProvider,
            out HttpClient backChannel)
        {
            if (oauthOptions.StateDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(AuthenticationManager).FullName,
                    typeof(string).FullName,
                    oauthOptions.AuthenticationScheme,
                    "v1");

                oauthOptions.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }
            
            backChannel = new HttpClient(oauthOptions.BackchannelHttpHandler ?? new HttpClientHandler());
            backChannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth middleware");
            backChannel.Timeout = oauthOptions.BackchannelTimeout;
            backChannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
        }

        #endregion
        
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
