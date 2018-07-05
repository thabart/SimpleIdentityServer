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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.Host.Configuration;
using SimpleIdentityServer.Uma.Host.Configurations;
using SimpleIdentityServer.Uma.Host.Services;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Linq;

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaHost(this IServiceCollection services, UmaHostConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // 1. Add the dependencies.
            RegisterServices(services, configuration);
            return services;
        }

        public static AuthorizationOptions AddUmaSecurityPolicy(this AuthorizationOptions authorizationOptions)
        {
            if (authorizationOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationOptions));
            }

            authorizationOptions.AddPolicy("UmaProtection", policy =>
            {				
				policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScope = p.User.Claims.FirstOrDefault(c => c.Type == "scope");
                    if (claimRole == null && claimScope == null)
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" || claimScope != null && claimScope.Value == "uma_protection";
                });
            });
            return authorizationOptions;
        }

        private static void RegisterServices(IServiceCollection services, UmaHostConfiguration configuration)
        {
            services.AddSimpleIdServerUmaCore()
                .AddSimpleIdentityServerCore()
                .AddSimpleIdentityServerJwt()
                .AddIdServerClient();
            services.AddTechnicalLogging();
            services.AddOAuthLogging();
            services.AddUmaLogging();
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
            if (configuration.ConfigurationService == null)
            {
                services.AddTransient<IConfigurationService, DefaultConfigurationService>();
            }
            else
            {
                services.AddTransient(typeof(IConfigurationService), configuration.ConfigurationService);
            }

            if (configuration.PasswordService == null)
            {
                services.AddTransient<IPasswordService, DefaultPasswordService>();
            }
            else
            {
                services.AddTransient(typeof(IPasswordService), configuration.PasswordService);
            }
        }
    }
}
