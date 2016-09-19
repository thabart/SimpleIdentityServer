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

using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Configuration.Controllers;
using SimpleIdentityServer.Logging;
using System.Reflection;

namespace SimpleIdentityServer.Configuration.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void UseConfigurationService(this IServiceCollection services)
        {
            // Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("display", policy => policy.RequireClaim("scope", "display_configuration"));
                options.AddPolicy("manage", policy => policy.RequireClaim("scope", "manage_configuration"));
            });
            Assembly assembly = null;
#if NET
            assembly = typeof(AuthProviderController).Assembly;
#else
            assembly = typeof(AuthProviderController).GetTypeInfo().Assembly;
#endif

            // Add the dependencies needed to run MVC
            services.AddMvc().AddApplicationPart(assembly);

            services.AddLogging();
            services.AddTransient<IConfigurationEventSource, ConfigurationEventSource>();
        }
    }
}
