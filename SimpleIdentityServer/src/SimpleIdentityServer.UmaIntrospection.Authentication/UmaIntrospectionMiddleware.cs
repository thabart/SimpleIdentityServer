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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using SimpleIdentityServer.Authentication.Common.Authentication;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Uma.Common;
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
            RegisterDependencies(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            // Create empty middleware
            var nullAuthenticationBuilder = app.New();
            var nullAuthenticationOptions = new NullAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            };
            nullAuthenticationBuilder.UseMiddleware<NullAuthenticationMiddleware>(nullAuthenticationOptions);
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
                await _nullAuthenticationNext(context);
            }

            Uri umaConfigurationUri = null;
            if (!Uri.TryCreate(_options.UmaConfigurationUrl, UriKind.Absolute, out umaConfigurationUri))
            {
                throw new ArgumentException($"url {_options.UmaConfigurationUrl} is not well formatted");
            }

            // Validate RPT
            var identityServerUmaClientFactory = (IIdentityServerUmaClientFactory)_serviceProvider.GetService(typeof(IIdentityServerUmaClientFactory));
            var introspectionResponse = await identityServerUmaClientFactory
                .GetIntrospectionClient()
                .GetIntrospectionByResolvingUrlAsync(rpt, umaConfigurationUri);
            // Add the permissions
            if (introspectionResponse.IsActive)
            {
                AddPermissions(context, introspectionResponse.Permissions);
            }

            await _nullAuthenticationNext(context);
        }

        #endregion

        #region Private static methods

        private static void AddPermissions(HttpContext context, List<PermissionResponse> permissions)
        {
            var claimsIdentity = context.User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return;
            }
            
            foreach(var permission in permissions)
            {
                var claimPermission = new Permission
                {
                    ResourceSetId = permission.ResourceSetId,
                    Scopes = permission.Scopes
                };

                claimsIdentity.AddPermission(claimPermission);
            }
        }

        private static string GetRpt(IHeaderDictionary header)
        {
            if (!header.ContainsKey(AuthorizationName))
            {
                return null;
            }

            var authorizationValues = header[AuthorizationName];
            return authorizationValues.FirstOrDefault();
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IIdentityServerClientFactory, IdentityServerClientFactory>();
            serviceCollection.AddTransient<IIdentityServerUmaClientFactory, IdentityServerUmaClientFactory>();
        }

        #endregion
    }
}
