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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Handlers
{
    public interface IAuthenticationManager
    {
        Task Initialize(HttpContext httpContext);
    }

    internal class AuthenticationManager : IAuthenticationManager
    {
        private Dictionary<string, Action<List<Option>, HttpContext, ILogger, IDataProtectionProvider>> _mappingNameToCallback = new Dictionary<string, Action<List<Option>, HttpContext, ILogger, IDataProtectionProvider>>
        {
            {
                "Facebook", EnableFacebookAuthentication
            },
            {
                "Microsoft", EnableMicrosoftAuthentication
            }
        };

        private readonly ISimpleIdServerConfigurationClientFactory _simpleIdServerConfigurationClientFactory;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly IAuthenticationOptionsProvider _authenticationOptionsProvider;

        private readonly ILogger _logger;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        #region Constructor

        public AuthenticationManager(
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            IIdentityServerClientFactory identityServerClientFactory,
            IAuthenticationOptionsProvider authenticationOptionsProvider,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
        {
            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _authenticationOptionsProvider = authenticationOptionsProvider;
            _logger = loggerFactory.CreateLogger("authentication");
            _dataProtectionProvider = dataProtectionProvider;
        }

        #endregion

        #region Public methods

        public async Task Initialize(HttpContext httpContext)
        {
            var url = httpContext.Request.GetAbsoluteUriWithVirtualPath();
            var options = _authenticationOptionsProvider.GetOptions();
            var grantedToken = await _identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth(options.ClientId, options.ClientSecret)
                .UseClientCredentials("display_configuration")
                .ExecuteAsync(url + "/" + Constants.EndPoints.Token);
            var authProviders = await _simpleIdServerConfigurationClientFactory.GetAuthProviderClient()
                .GetAuthProviders(new Uri(options.ConfigurationUrl + "/authproviders"), grantedToken.AccessToken);
            foreach(var authProvider in authProviders)
            {
                if (authProvider.IsEnabled && _mappingNameToCallback.ContainsKey(authProvider.Name))
                {
                    _mappingNameToCallback[authProvider.Name](authProvider.Options, httpContext, _logger, _dataProtectionProvider);
                }
            }
        }

        #endregion

        #region Enable authentication methods

        private static void EnableFacebookAuthentication(List<Option> options, HttpContext context, ILogger logger, IDataProtectionProvider dataProtectionProvider)
        {
            var option = ExtractFacebookOptions(options, dataProtectionProvider);
            if (option == null)
            {
                return;
            }

            var handler = new FacebookHandler(new HttpClient());
            handler.InitializeAsync(option, context, logger, UrlEncoder.Default);
        }

        private static void EnableMicrosoftAuthentication(List<Option> options, HttpContext httpContext, ILogger logger, IDataProtectionProvider dataProtectionProvider)
        {
            var microsoftOptions = ExtractMicrosoftOptions(options, dataProtectionProvider);
            if (microsoftOptions == null)
            {
                return;
            }

            microsoftOptions.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    // 1. Fetch the user information from the user-information endpoint
                    var request = new HttpRequestMessage(HttpMethod.Get, microsoftOptions.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

                    // 2. Retrieve the subject
                    var identifier = MicrosoftAccountHelper.GetId(payload);
                    if (!string.IsNullOrWhiteSpace(identifier))
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim(
                            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                            identifier, ClaimValueTypes.String, context.Options.ClaimsIssuer
                        ));
                    }

                    // 3. Retrieve the name
                    var name = MicrosoftAccountHelper.GetGivenName(payload);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim(
                            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                            name, ClaimValueTypes.String, context.Options.ClaimsIssuer
                        ));
                    }

                    // 3. Retrieve the email
                    var email = MicrosoftAccountHelper.GetEmail(payload);
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim(
                            Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                            email, ClaimValueTypes.String, context.Options.ClaimsIssuer
                        ));
                    }
                }
            };

            var handler = new OAuthHandler<OAuthOptions>(new HttpClient());
            handler.InitializeAsync(microsoftOptions, httpContext, logger, UrlEncoder.Default);
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

        private static OAuthOptions ExtractMicrosoftOptions(List<Option> options, IDataProtectionProvider dataProtectionProvider)
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

            var microsoftAccountOptions = new OAuthOptions
            {
                AuthenticationScheme = Constants.IdentityProviderNames.Microsoft,
                DisplayName = Constants.IdentityProviderNames.Microsoft,
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value,
                CallbackPath = new PathString("/signin-microsoft"),
                AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint,
                TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint,
                UserInformationEndpoint = MicrosoftAccountDefaults.UserInformationEndpoint,
                StateDataFormat = new PropertiesDataFormat(dataProtector)
            };
            foreach (var scope in scopes.Select(s => s.Value))
            {
                microsoftAccountOptions.Scope.Add(scope);
            }

            return microsoftAccountOptions;
        }

        #endregion
    }
}
