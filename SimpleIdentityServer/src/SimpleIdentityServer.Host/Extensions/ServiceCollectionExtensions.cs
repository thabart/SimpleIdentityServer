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
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.TwoFactors;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Host.TwoFactors;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.RateLimitation;
using System;

namespace SimpleIdentityServer.Host
{
    public enum DataSourceTypes 
    {
        SqlServer,
        SqlLite,
        Postgre,
        InMemory
    }
    
    public sealed class DataSourceOptions
    {        
        /// <summary>
        /// Choose the type of your DataSource
        /// </summary>
        public DataSourceTypes DataSourceType { get; set;}
        
         /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString { get; set;}
    }

    public sealed class LoggingOptions
    {
        public FileLogOptions FileLogOptions { get; set; }

        public ElasticsearchOptions ElasticsearchOptions { get; set; }
    }

    public sealed class FileLogOptions
    {
        #region Constructor

        public FileLogOptions()
        {
            PathFormat = "log-{Date}.txt";
        }

        #endregion

        #region Properties

        public bool IsEnabled { get; set; }

        public string PathFormat { get; set; }

        #endregion
    }

    public sealed class ElasticsearchOptions
    {
        #region Constructor

        public ElasticsearchOptions()
        {
            Url = "http://localhost:9200";
        }

        #endregion

        #region Properties

        public bool IsEnabled { get; set; }

        public string Url { get; set; }

        #endregion
    }
        
    public static class ServiceCollectionExtensions 
    {
        #region Public static methods
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            Action<DataSourceOptions> dataSourceCallback,
            Action<LoggingOptions> loggingOptionsCallback,
            string configurationUrl) 
        {
            if (dataSourceCallback == null)
            {
                throw new ArgumentNullException(nameof(dataSourceCallback));
            }

            if (loggingOptionsCallback == null)
            {
                throw new ArgumentNullException(nameof(loggingOptionsCallback));
            }

            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }
            
            var dataSourceOptions = new DataSourceOptions();
            var loggingOptions = new LoggingOptions();
            dataSourceCallback(dataSourceOptions);
            loggingOptionsCallback(loggingOptions);
            serviceCollection.AddSimpleIdentityServer(
                dataSourceOptions,
                loggingOptions,
                configurationUrl);
        }
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection, 
            DataSourceOptions dataSourceOptions,
            LoggingOptions loggingOptions,
            string configurationUrl) 
        {
            if (dataSourceOptions == null) {
                throw new ArgumentNullException(nameof(dataSourceOptions));
            }           

            if (loggingOptions == null)
            {
                throw new ArgumentNullException(nameof(loggingOptions));
            }

            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            switch(dataSourceOptions.DataSourceType)
            {
                case DataSourceTypes.SqlServer:
                    serviceCollection.AddSimpleIdentityServerSqlServer(dataSourceOptions.ConnectionString);
                    break;
                case DataSourceTypes.SqlLite:
                    serviceCollection.AddSimpleIdentityServerSqlLite(dataSourceOptions.ConnectionString);
                    break;
                case DataSourceTypes.Postgre:
                    serviceCollection.AddSimpleIdentityServerPostgre(dataSourceOptions.ConnectionString);
                    break;
                case DataSourceTypes.InMemory:
                    serviceCollection.AddSimpleIdentityServerInMemory();
                    break;
                default:
                    throw new ArgumentException($"the data source '{dataSourceOptions.DataSourceType}' type is not supported");
            }
            
            ConfigureSimpleIdentityServer(
                serviceCollection, 
                loggingOptions,
                configurationUrl);
        }
        
        #endregion
        
        #region Private static methods
        
        /// <summary>
        /// Add all the dependencies needed to run Simple Identity Server
        /// </summary>
        /// <param name="services"></param>
        /// <param name="swaggerOptions"></param>
        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            LoggingOptions loggingOptions,
            string configurationUrl) 
        {
            services.AddSimpleIdentityServerCore();
            services.AddSimpleIdentityServerJwt();
            services.AddRateLimitation();
            services.AddIdServerClient();
            services.AddConfigurationClient();
            services.AddDataProtection();
            services.AddTransient<ICertificateStore, CertificateStore>();
            services.AddTransient<IResourceOwnerService, InMemoryUserService>();
            services.AddTransient<IRedirectInstructionParser, RedirectInstructionParser>();
            services.AddTransient<IActionResultParser, ActionResultParser>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IEncryptedPasswordFactory, EncryptedPasswordFactory>();

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
            var twoFactorServiceStore = new TwoFactorServiceStore();
            var factory = new SimpleIdServerConfigurationClientFactory();
            twoFactorServiceStore.Add(new TwilioSmsService(factory, configurationUrl));
            twoFactorServiceStore.Add(new EmailService(factory, configurationUrl));
            services.AddSingleton<ITwoFactorServiceStore>(twoFactorServiceStore);
        }
        
        #endregion
    }
}