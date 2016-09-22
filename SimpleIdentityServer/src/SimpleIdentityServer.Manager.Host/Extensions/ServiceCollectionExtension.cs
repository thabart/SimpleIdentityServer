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
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core;
using System;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Client;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddSimpleIdentityServerManager(
            this IServiceCollection serviceCollection,
            AuthorizationServerOptions authorizationServerOptions,
            LoggingOptions loggingOptions)
        {
            if (authorizationServerOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationServerOptions));
            }

            // Add the dependencies needed to enable CORS
            serviceCollection.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));            

            serviceCollection.AddSimpleIdentityServerCore();
            serviceCollection.AddSimpleIdentityServerManagerCore();
            serviceCollection.AddConfigurationClient();
            serviceCollection.AddIdServerClient();

            // Add authentication
            serviceCollection.AddAuthentication();

            // Add authorization policy rules
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("manager", policy => policy.RequireClaim("scope", "openid_manager"));
            });

            serviceCollection.AddSimpleIdentityServerJwt();

            // Add the dependencies needed to run MVC
            serviceCollection.AddMvc();

            // Configure SeriLog pipeline
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
            if (loggingOptions.FileLogOptions != null &&
                loggingOptions.FileLogOptions.IsEnabled)
            {
                logger.WriteTo.RollingFile(loggingOptions.FileLogOptions.PathFormat);
            }

            if (loggingOptions.ElasticsearchOptions != null &&
                loggingOptions.ElasticsearchOptions.IsEnabled)
            {
                logger.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(loggingOptions.ElasticsearchOptions.Url))
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
