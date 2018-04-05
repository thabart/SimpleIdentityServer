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
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core;
using SimpleIdentityServer.Manager.Host.Services;
using System;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddSimpleIdentityServerManager(
            this IServiceCollection serviceCollection,
            ManagerOptions managerOptions)
        {
            if (managerOptions == null)
            {
                throw new ArgumentNullException(nameof(managerOptions));
            }

            if (managerOptions.Logging == null)
            {
                throw new ArgumentNullException(nameof(managerOptions.Logging));
            }

            if (managerOptions.PasswordService == null)
            {
                serviceCollection.AddTransient<IPasswordService, DefaultPasswordService>();
            }
            else
            {
                serviceCollection.AddSingleton(managerOptions.PasswordService);
            }

            if (managerOptions.AuthenticateResourceOwnerService == null)
            {
                serviceCollection.AddTransient<IAuthenticateResourceOwnerService, DefaultAuthenticateResourceOwerService>();
            }
            else
            {
                serviceCollection.AddSingleton(managerOptions.AuthenticateResourceOwnerService);
            }

            // 1. Add the dependencies needed to enable CORS
            serviceCollection.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 2. Register all the dependencies.
            serviceCollection.AddSimpleIdentityServerCore();
            serviceCollection.AddSimpleIdentityServerManagerCore();
            // 3. Register the dependencies to run the authentication.
            serviceCollection.AddAuthentication();
            // 4. Add authorization policies
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("manager", policy => policy.RequireClaim("scope", "manager"));
            });
            // 5. Add JWT parsers.
            serviceCollection.AddSimpleIdentityServerJwt();
            // 6. Add the dependencies needed to run MVC
            serviceCollection.AddMvc();
            // 7. Configure Serilog
            Func<LogEvent, bool> serilogFilter = (e) =>
            {
                var ctx = e.Properties["SourceContext"];
                var contextValue = ctx.ToString()
                    .TrimStart('"')
                    .TrimEnd('"');
                return contextValue.StartsWith("SimpleIdentityServer") ||
                    e.Level == LogEventLevel.Error ||
                    e.Level == LogEventLevel.Fatal;
            };
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole();
            if (managerOptions.Logging.FileLogOptions != null &&
                managerOptions.Logging.FileLogOptions.IsEnabled)
            {
                logger.WriteTo.RollingFile(managerOptions.Logging.FileLogOptions.PathFormat);
            }
            if (managerOptions.Logging.ElasticsearchOptions != null &&
                managerOptions.Logging.ElasticsearchOptions.IsEnabled)
            {
                logger.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(managerOptions.Logging.ElasticsearchOptions.Url))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "manager-{0:yyyy.MM.dd}",
                    TemplateName = "manager-events-template"
                });
            }

            var log = logger.Filter.ByIncludingOnly(serilogFilter)
                .CreateLogger();
            Log.Logger = log;
            serviceCollection.AddLogging();
            serviceCollection.AddTransient<IManagerEventSource, ManagerEventSource>();
            serviceCollection.AddTransient<ISimpleIdentityServerEventSource, SimpleIdentityServerEventSource>();
        }
    }
}
