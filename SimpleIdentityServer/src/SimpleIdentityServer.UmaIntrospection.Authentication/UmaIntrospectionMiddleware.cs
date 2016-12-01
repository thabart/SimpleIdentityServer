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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleIdentityServer.Authentication.Common.Authentication;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Uma.Common;
using SimpleIdentityServer.UmaManager.Client;
using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UmaIntrospection.Authentication
{
    internal class UmaIntrospectionMiddleware<TOptions> where TOptions : UmaIntrospectionOptions, new()
    {
        private const string AuthorizationName = "Authorization";
        private const string BearerName = "Bearer";
        private readonly RequestDelegate _next;
        private readonly IApplicationBuilder _app;
        private readonly UmaIntrospectionOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly RequestDelegate _nullAuthenticationNext;

        #region Constructor

        public UmaIntrospectionMiddleware(
            RequestDelegate next,
            IApplicationBuilder app,
            IOptions<TOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _app = app;
            _options = options.Value;            
            // Register dependencies
            var serviceCollection = new ServiceCollection();
            RegisterDependencies(serviceCollection, _options);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            // Create empty middleware
            var nullAuthenticationBuilder = app.New();
            var nullAuthenticationOptions = new NullAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            };
            nullAuthenticationBuilder.UseMiddleware<NullAuthenticationMiddleware>(Options.Create(nullAuthenticationOptions));
            nullAuthenticationBuilder.Run(ctx => next(ctx));
            _nullAuthenticationNext = nullAuthenticationBuilder.Build();
        }
        
        #endregion

        #region Public methods

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers;
            var rpt = GetRpt(headers);
            if (string.IsNullOrWhiteSpace(rpt))
            {
                // FAKE USER
                await _nullAuthenticationNext(context);
                return;
            }

            Uri umaConfigurationUri = null;
            if (!Uri.TryCreate(_options.UmaConfigurationUrl, UriKind.Absolute, out umaConfigurationUri))
            {
                throw new ArgumentException($"url {_options.UmaConfigurationUrl} is not well formatted");
            }

            Uri resourceUrl = null;
            if (!Uri.TryCreate(_options.ResourcesUrl, UriKind.Absolute, out resourceUrl))
            {
                throw new ArgumentException($"url {_options.ResourcesUrl} is not well formatted");
            }

            // Validate RPT
            var identityServerUmaClientFactory = (IIdentityServerUmaClientFactory)_serviceProvider.GetService(typeof(IIdentityServerUmaClientFactory));
            try
            {
                var introspectionResponse = await identityServerUmaClientFactory
                    .GetIntrospectionClient()
                    .GetIntrospectionByResolvingUrlAsync(rpt, umaConfigurationUri);
                // Add the permissions
                if (introspectionResponse.IsActive)
                {
                    await AddPermissions(context, introspectionResponse.Permissions);
                }
            }
            catch (Exception)
            {
            }

            await _nullAuthenticationNext(context);
        }

        #endregion

        #region Private methods
        
        private async Task AddPermissions(
            HttpContext context, 
            List<PermissionResponse> permissions)
        {
            var claims = new List<Claim>();
            var claimsIdentity = new ClaimsIdentity(claims, "UserInformation");
            if (context.User != null &&
                context.User.Identity != null &&
                context.User.Identity.IsAuthenticated)
            {
                claimsIdentity = context.User.Identity as ClaimsIdentity;
            }

            var accessToken = await GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return;
            }

            var identityServerUmaManagerClientFactory = (IIdentityServerUmaManagerClientFactory)_serviceProvider.GetService(typeof(IIdentityServerUmaManagerClientFactory));
            foreach (var permission in permissions)
            {
                var claimPermission = new Permission
                {
                    ResourceSetId = permission.ResourceSetId,
                    Scopes = permission.Scopes
                };

                var resourceClient = identityServerUmaManagerClientFactory.GetResourceClient();
                var operations = await resourceClient
                    .SearchResources(new SearchResourceRequest
                    {
                        ResourceId = permission.ResourceSetId
                    }, _options.ResourcesUrl, accessToken);
                if (operations != null && operations.Any())
                {
                    var operation = operations.First();
                    claimPermission.Url = operation.Url;
                }
                
                claimsIdentity.AddPermission(claimPermission);
            }

            context.User = new ClaimsPrincipal(claimsIdentity);
        }

        private async Task<string> GetAccessToken()
        {
            try
            {
                var serverClientFactory = (IIdentityServerClientFactory)_serviceProvider.GetService(typeof(IIdentityServerClientFactory));
                var response = await serverClientFactory.CreateTokenClient()
                    .UseClientSecretPostAuth(_options.ClientId, _options.ClientSecret)
                    .UseClientCredentials("website_api")
                    .ResolveAsync(_options.OpenIdWellKnownConfigurationUrl);
                if (response == null)
                {
                    return null;
                }

                return response.AccessToken;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Private static methods

        private static string GetRpt(IHeaderDictionary header)
        {
            if (!header.ContainsKey(AuthorizationName))
            {
                return null;
            }

            var authorizationValues = header[AuthorizationName];
            var authorizationValue = authorizationValues.FirstOrDefault();
            var splittedAuthorizationValue = authorizationValue.Split(' ');
            if (splittedAuthorizationValue.Count() == 2 &&
                splittedAuthorizationValue[0].Equals(BearerName, StringComparison.CurrentCultureIgnoreCase))
            {
                return splittedAuthorizationValue[1];
            }

            return null;
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection, UmaIntrospectionOptions options)
        {
            if (options.IdentityServerUmaClientFactory != null)
            {
                serviceCollection.AddSingleton(options.IdentityServerUmaClientFactory);
            }
            else
            {
                serviceCollection.AddTransient<IIdentityServerUmaClientFactory, IdentityServerUmaClientFactory>();
            }

            if (options.IdentityServerUmaManagerClientFactory != null)
            {
                serviceCollection.AddSingleton(options.IdentityServerUmaManagerClientFactory);
            }
            else
            {
                serviceCollection.AddTransient<IIdentityServerUmaManagerClientFactory, IdentityServerUmaManagerClientFactory>();
            }

            if (options.IdentityServerClientFactory != null)
            {
                serviceCollection.AddSingleton(options.IdentityServerClientFactory);
            }
            else
            {
                serviceCollection.AddTransient<IIdentityServerClientFactory, IdentityServerClientFactory>();
            }
        }

        #endregion
    }
}
