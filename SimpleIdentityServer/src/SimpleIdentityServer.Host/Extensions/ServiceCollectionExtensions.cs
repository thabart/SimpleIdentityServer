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
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.EventStore.EF;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Host.Services;
using SimpleIdentityServer.Logging;
using System;

namespace SimpleIdentityServer.Host
{
    public static class ServiceCollectionExtensions 
    {
        public static IServiceCollection AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            Action<IdentityServerOptions> optionsCallback)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }
            
            var options = new IdentityServerOptions();
            optionsCallback(options);
            serviceCollection.AddSimpleIdentityServer(
                options);
            return serviceCollection;
        }
        
        public static IServiceCollection AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

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

            switch(options.DataSource.OpenIdDataSourceType)
            {
                case DataSourceTypes.SqlServer:
                    serviceCollection.AddSimpleIdentityServerSqlServer(options.DataSource.OpenIdConnectionString);
                    break;
                case DataSourceTypes.SqlLite:
                    serviceCollection.AddSimpleIdentityServerSqlLite(options.DataSource.OpenIdConnectionString);
                    break;
                case DataSourceTypes.Postgre:
                    serviceCollection.AddSimpleIdentityServerPostgre(options.DataSource.OpenIdConnectionString);
                    break;
                case DataSourceTypes.InMemory:
                    serviceCollection.AddSimpleIdentityServerInMemory();
                    break;
                default:
                    throw new ArgumentException($"the data source '{options.DataSource.OpenIdDataSourceType}' type is not supported");
            }

            switch(options.DataSource.EvtStoreDataSourceType)
            {
                case DataSourceTypes.SqlServer:
                    serviceCollection.AddEventStoreSqlServer(options.DataSource.EvtStoreConnectionString);
                    break;
                case DataSourceTypes.SqlLite:
                    serviceCollection.AddEventStoreSqlLite(options.DataSource.EvtStoreConnectionString);
                    break;
                case DataSourceTypes.Postgre:
                    serviceCollection.AddEventStorePostgre(options.DataSource.EvtStoreConnectionString);
                    break;
                case DataSourceTypes.InMemory:
                    serviceCollection.AddEventStoreInMemory();
                    break;
            }
            
            ConfigureSimpleIdentityServer(
                serviceCollection, 
                options);
            return serviceCollection;
        }

        public static IServiceCollection AddHostIdentityServer(this IServiceCollection serviceCollection, IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.AuthenticateResourceOwner == null)
            {
                serviceCollection.AddTransient<IAuthenticateResourceOwnerService, DefaultAuthenticateResourceOwerService>();
            }
            else
            {
                serviceCollection.AddTransient(typeof(IAuthenticateResourceOwnerService), options.AuthenticateResourceOwner);
            }

            if (options.ConfigurationService == null)
            {
                serviceCollection.AddTransient<IConfigurationService, DefaultConfigurationService>();
            }
            else
            {
                serviceCollection.AddTransient(typeof(IConfigurationService), options.ConfigurationService);
            }

            if (options.PasswordService == null)
            {
                serviceCollection.AddTransient<IPasswordService, DefaultPasswordService>();
            }
            else
            {
                serviceCollection.AddTransient(typeof(IPasswordService), options.PasswordService);
            }

            if (options.TwoFactorServiceStore == null)
            {
                serviceCollection.AddTransient<ITwoFactorServiceStore, TwoFactorServiceStore>();
            }
            else
            {
                serviceCollection.AddSingleton<ITwoFactorServiceStore>(options.TwoFactorServiceStore);
            }

            serviceCollection
                .AddSingleton(options.Authenticate)
                .AddSingleton(options.Scim)
                .AddTransient<ICertificateStore, CertificateStore>()
                .AddTransient<IRedirectInstructionParser, RedirectInstructionParser>()
                .AddTransient<IActionResultParser, ActionResultParser>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddDataProtection();

            return serviceCollection;
        }
                
        /// <summary>
        /// Add all the dependencies needed to run Simple Identity Server
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            IdentityServerOptions options)
        {
            services.AddSimpleIdentityServerCore()
                .AddSimpleIdentityServerJwt()
                .AddHostIdentityServer(options)
                .AddIdServerClient()
                .AddConfigurationClient()
                .AddIdServerLogging()
                .AddBus(options)
                .AddDataProtection();

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
            services.AddSingleton<ILogger>(log);
        }

        private static IServiceCollection AddBus(this IServiceCollection services,
            IdentityServerOptions options)
        {
            if (options.Event == null || options.Event.Publisher == null)
            {
                var handlers = options.Event == null ? null : options.Event.Handlers;
                services.AddDefaultBus(handlers);
                return services;
            }

            services.AddTransient(typeof(IEventPublisher), options.Event.Publisher);
            return services;
        }
    }
}