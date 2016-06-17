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
        Task<bool> Initialize(HttpContext httpContext, AuthenticationOptions authenticationOptions);
    }

    internal class AuthenticationManager : IAuthenticationManager
    {
        private readonly ISimpleIdServerConfigurationClientFactory _simpleIdServerConfigurationClientFactory;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly ILogger _logger;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        #region Constructor

        public AuthenticationManager(
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            IIdentityServerClientFactory identityServerClientFactory,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
        {
            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _logger = loggerFactory.CreateLogger("authentication");
            _dataProtectionProvider = dataProtectionProvider;
        }

        #endregion

        #region Public methods

        public async Task<bool> Initialize(HttpContext httpContext, AuthenticationOptions authenticationOptions)
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
            var microsoftOptions = ExtractMicrosoftOptions(options, _dataProtectionProvider);
            if (microsoftOptions == null)
            {
                return false;
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
            await handler.InitializeAsync(microsoftOptions, httpContext, _logger, UrlEncoder.Default);
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
