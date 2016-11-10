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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Host.Services;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.RateLimitation;
using System;

namespace SimpleIdentityServer.Host
{
    public static class ServiceCollectionExtensions 
    {        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            Action<IdentityServerOptions> optionsCallback) 
        {
            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }
            
            var options = new IdentityServerOptions();
            optionsCallback(options);
            serviceCollection.AddSimpleIdentityServer(
                options);
        }
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            IdentityServerOptions options) 
        {
            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }           

            if (options.Logging == null)
            {
                throw new ArgumentNullException(nameof(options.Logging));
            }

            if (options.DataSource == null)
            {
                throw new ArgumentNullException(nameof(options.DataSource));
            }

            switch(options.DataSource.DataSourceType)
            {
                case DataSourceTypes.SqlServer:
                    serviceCollection.AddSimpleIdentityServerSqlServer(options.DataSource.ConnectionString);
                    break;
                case DataSourceTypes.SqlLite:
                    serviceCollection.AddSimpleIdentityServerSqlLite(options.DataSource.ConnectionString);
                    break;
                case DataSourceTypes.Postgre:
                    serviceCollection.AddSimpleIdentityServerPostgre(options.DataSource.ConnectionString);
                    break;
                case DataSourceTypes.InMemory:
                    serviceCollection.AddSimpleIdentityServerInMemory();
                    break;
                default:
                    throw new ArgumentException($"the data source '{options.DataSource.DataSourceType}' type is not supported");
            }
            
            ConfigureSimpleIdentityServer(
                serviceCollection, 
                options);
        }
                
        /// <summary>
        /// Add all the dependencies needed to run Simple Identity Server
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            IdentityServerOptions options)
        {
            services.AddSimpleIdentityServerCore();
            services.AddSimpleIdentityServerJwt();
            services.AddRateLimitation();
            services.AddIdServerClient();
            services.AddConfigurationClient();
            services.AddDataProtection();
            if (options.AuthenticateResourceOwner == null)
            {
                services.AddTransient<IAuthenticateResourceOwnerService, DefaultAuthenticateResourceOwerService>();
            }
            else
            {
                services.AddSingleton(options.AuthenticateResourceOwner);
            }

            if (options.TwoFactorServiceStore == null)
            {
                services.AddTransient<ITwoFactorServiceStore, TwoFactorServiceStore>();
            }
            else
            {
                services.AddSingleton(options.TwoFactorServiceStore);
            }

            if (options.ConfigurationService == null)
            {
                services.AddTransient<IConfigurationService, DefaultConfigurationService>();
            }
            else
            {
                services.AddSingleton(options.ConfigurationService);
            }

            if (options.PasswordService == null)
            {
                services.AddTransient<IPasswordService, DefaultPasswordService>();
            }
            else
            {
                services.AddSingleton(options.PasswordService);
            }

            services.AddTransient<ICertificateStore, CertificateStore>();
            services.AddTransient<IRedirectInstructionParser, RedirectInstructionParser>();
            services.AddTransient<IActionResultParser, ActionResultParser>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

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
            if (options.Logging.FileLogOptions != null &&
                options.Logging.FileLogOptions.IsEnabled)
            {
                logger.WriteTo.RollingFile(options.Logging.FileLogOptions.PathFormat);
            }

            if (options.Logging.ElasticsearchOptions != null &&
                options.Logging.ElasticsearchOptions.IsEnabled)
            {
                logger.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.Logging.ElasticsearchOptions.Url))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "simpleidserver-{0:yyyy.MM.dd}"
                });
            }
        
            var log = logger.Filter.ByIncludingOnly(serilogFilter)
                .CreateLogger();
            Log.Logger = log;
            services.AddLogging();
            services.AddTransient<ISimpleIdentityServerEventSource, SimpleIdentityServerEventSource>();
            services.AddTransient<IManagerEventSource, ManagerEventSource>();
            services.AddSingleton<ILogger>(log);
            /*
            var twoFactorServiceStore = new TwoFactorServiceStore();
            var factory = new SimpleIdServerConfigurationClientFactory();
            twoFactorServiceStore.Add(new TwilioSmsService(factory, configurationUrl));
            twoFactorServiceStore.Add(new EmailService(factory, configurationUrl));
            services.AddSingleton<ITwoFactorServiceStore>(twoFactorServiceStore);
            */
        }
    }
}