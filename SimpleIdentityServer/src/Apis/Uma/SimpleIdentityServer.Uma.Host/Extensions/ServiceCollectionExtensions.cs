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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.Host.Configuration;
using SimpleIdentityServer.Uma.Host.Configurations;
using SimpleIdentityServer.Uma.Host.Services;
using SimpleIdentityServer.Uma.Logging;
using System;

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
            // 2. Add authorization policies.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UmaProtection", policy => policy.RequireClaim("scope", "uma_protection"));
            });
            // 3. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 4. Add authentication.
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API.
            services.AddMvc();
            return services;
        }

        private static void RegisterServices(IServiceCollection services, UmaHostConfiguration configuration)
        {
            var parametersProvider = new ParametersProvider(configuration.OpenIdWellKnownConfiguration);
            services.AddSimpleIdServerUmaCore()
                .AddSimpleIdentityServerCore()
                .AddSimpleIdentityServerJwt()
                .AddIdServerClient();
            services.AddIdServerLogging();
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IParametersProvider>(parametersProvider);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
            if (configuration.AuthenticateResourceOwner == null)
            {
                services.AddTransient<IAuthenticateResourceOwnerService, DefaultAuthenticateResourceOwerService>();
            }
            else
            {
                services.AddTransient(typeof(IAuthenticateResourceOwnerService), configuration.AuthenticateResourceOwner);
            }

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
