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
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.EventStore.EF;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Store.Redis;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.Host.Configuration;
using SimpleIdentityServer.Uma.Host.Configurations;
using SimpleIdentityServer.Uma.Host.Services;
using SimpleIdentityServer.Uma.Logging;
using System;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

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

            // 1. Enable caching.
            if (configuration.ResourceCaching.Type == CachingTypes.REDIS)
            {
                services.AddConcurrency(opt => opt.UseRedis(o =>
                {
                    o.Configuration = configuration.ResourceCaching.ConnectionString;
                    o.InstanceName = configuration.ResourceCaching.InstanceName;
                }));
            }
            else
            {
                services.AddConcurrency(opt => opt.UseInMemory());
            }

            // 2. Enable database.
            switch(configuration.DataSource.UmaDbType)
            {
                case DbTypes.SQLSERVER:
                    services.AddSimpleIdServerUmaSqlServer(configuration.DataSource.UmaConnectionString);
                    break;
                case DbTypes.POSTGRES:
                    services.AddSimpleIdServerUmaPostgresql(configuration.DataSource.UmaConnectionString);
                    break;
                case DbTypes.INMEMORY:
                    services.AddSimpleIdServerUmaInMemory();
                    break;
            }

            switch (configuration.DataSource.OauthDbType)
            {
                case DbTypes.SQLSERVER:
                    services.AddSimpleIdentityServerSqlServer(configuration.DataSource.OauthConnectionString);
                    break;
                case DbTypes.POSTGRES:
                    services.AddSimpleIdentityServerPostgre(configuration.DataSource.OauthConnectionString);
                    break;
                case DbTypes.INMEMORY:
                    services.AddSimpleIdentityServerInMemory();
                    break;
            }

            switch (configuration.DataSource.EvtStoreDataSourceType)
            {
                case DbTypes.SQLSERVER:
                    services.AddEventStoreSqlServer(configuration.DataSource.EvtStoreConnectionString);
                    break;
                case DbTypes.POSTGRES:
                    services.AddEventStorePostgre(configuration.DataSource.EvtStoreConnectionString);
                    break;
                case DbTypes.INMEMORY:
                    services.AddEventStoreInMemory();
                    break;
            }

            switch(configuration.Storage.Type)
            {
                case CachingTypes.REDIS:
                    services.AddRedisStores((opts) =>
                    {
                        opts.Configuration = configuration.Storage.ConnectionString;
                        opts.InstanceName = configuration.Storage.InstanceName;
                    }, configuration.Storage.Port);
                    // TODO : implement the REDIS CACHE FOR UMA.
                    break;
                case CachingTypes.INMEMORY:
                    services.AddInMemoryStores();
                    services.AddUmaInMemoryStore();
                    break;
            }
            
            // 3. Add the default bus.
            services.AddDefaultBus();

            // 3. Enable logging.
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole();
            if (configuration.FileLog.IsEnabled)
            {
                logger.WriteTo.RollingFile(configuration.FileLog.PathFormat);
            }

            if (configuration.Elasticsearch.IsEnabled)
            {
                logger.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration.Elasticsearch.Url))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "umaserver-{0:yyyy.MM.dd}",
                    TemplateName = "uma-events-template"
                });
            }

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
            var log = logger.Filter.ByIncludingOnly(serilogFilter)
                .CreateLogger();
            Log.Logger = log;
            services.AddLogging();
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
