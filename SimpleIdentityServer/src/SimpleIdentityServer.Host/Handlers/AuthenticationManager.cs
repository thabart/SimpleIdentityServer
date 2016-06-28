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
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
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
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using SimpleIdentityServer.Host.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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

        private readonly HtmlEncoder _htmlEncoder;

        #region Constructor

        public AuthenticationManager(
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            IIdentityServerClientFactory identityServerClientFactory,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider,
            HtmlEncoder htmlEncoder)
        {
            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _logger = loggerFactory.CreateLogger("authentication");
            _dataProtectionProvider = dataProtectionProvider;
            _htmlEncoder = htmlEncoder;
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
            foreach(var authProvider in authProviders)
            {
                if (!authProvider.IsEnabled)
                {
                    continue;
                }

                if (authProvider.Name == "Facebook")
                {
                    if (await EnableFacebookAuthentication(authProvider.Options, httpContext))
                    {
                        return true;
                    }
                }
                else if (authProvider.Name == "Microsoft")
                {
                    if (await EnableMicrosoftAuthentication(authProvider.Options, httpContext))
                    {
                        return true;
                    }
                }
                else if (authProvider.Name == "ADFS")
                {
                    if (await EnableAdfsAuthentication(authProvider.Options, httpContext))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Enable authentication methods

        private async Task<bool> EnableFacebookAuthentication(List<Option> options, HttpContext context)
        {
            var option = ExtractFacebookOptions(options, _dataProtectionProvider);
            if (option == null)
            {
                return false;
            }

            var handler = new FacebookHandler(new HttpClient());
            await handler.InitializeAsync(option, context, _logger, UrlEncoder.Default);
            return await handler.HandleRequestAsync();
        }

        private async Task<bool> EnableMicrosoftAuthentication(List<Option> options, HttpContext httpContext)
        {
            HttpClient httpClient = null;
            var microsoftOptions = ExtractMicrosoftOptions(options, _dataProtectionProvider, out httpClient);
            if (microsoftOptions == null)
            {
                return false;
            }
            
            var handler = new OpenIdConnectHandler(httpClient, _htmlEncoder);
            await handler.InitializeAsync(microsoftOptions, httpContext, _logger, UrlEncoder.Default);
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

        #endregion

        #region Extract options

        private static FacebookOptions ExtractFacebookOptions(List<Option> options, IDataProtectionProvider dataProtectionProvider)
        {
            var scopes = options.Where(o => o.Key == "Scope").ToList();
            var clientId = options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = options.FirstOrDefault(o => o.Key == "ClientSecret");
            if (clientId == null || clientSecret == null || scopes == null || !scopes.Any())
            {
                return null;
            }

            var dataProtector = dataProtectionProvider.CreateProtector(
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Microsoft, "v1");

            var result = new FacebookOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Facebook,
                DisplayName = Constants.IdentityProviderNames.Facebook,
                AppId = clientId.Value,
                AppSecret = clientSecret.Value,
                StateDataFormat = new PropertiesDataFormat(dataProtector)
            };

            foreach(var scope in scopes.Select(s => s.Value))
            {
                result.Scope.Add(scope);
            }

            return result;
        }

        private static OpenIdConnectOptions ExtractMicrosoftOptions(
            List<Option> options, 
            IDataProtectionProvider dataProtectionProvider,
            out HttpClient httpClient)
        {
            httpClient = null;
            var scopes = options.Where(o => o.Key == "Scope").ToList();
            var clientId = options.FirstOrDefault(o => o.Key == "ClientId");
            var clientSecret = options.FirstOrDefault(o => o.Key == "ClientSecret");
            if (clientId == null || clientSecret == null || scopes == null || !scopes.Any())
            {
                return null;
            }
                        
            var dataProtector = dataProtectionProvider.CreateProtector(
                typeof(AuthenticationManager).FullName, Constants.IdentityProviderNames.Microsoft, "v1");

            var microsoftAccountOptions = new OpenIdConnectOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Microsoft,
                SignInScheme = Constants.IdentityProviderNames.Cookies,
                DisplayName = Constants.IdentityProviderNames.Microsoft,
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                StateDataFormat = new PropertiesDataFormat(dataProtector),
                Authority = "https://login.microsoftonline.com/common/v2.0",
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false
                }
            };

            microsoftAccountOptions.Scope.Clear();
            foreach (var scope in scopes.Select(s => s.Value))
            {
                microsoftAccountOptions.Scope.Add(scope);
            }

            EnrichOpenidOptions(microsoftAccountOptions, dataProtectionProvider, out httpClient);
            return microsoftAccountOptions;
        }

        private static OAuthOptions ExtractAdfsOptions(List<Option> options, IDataProtectionProvider dataProtectionProvider)
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
