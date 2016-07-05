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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RollingFile;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Host.Controllers;
using SimpleIdentityServer.Host.Handlers;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.RateLimitation;
using System;
using System.Reflection;

namespace SimpleIdentityServer.Host 
{    
    public enum DataSourceTypes 
    {
        SqlServer,
        SqlLite,
        Postgre
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
        
    public static class ServiceCollectionExtensions 
    {
        #region Public static methods
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection,
            Action<DataSourceOptions> dataSourceCallback,
            Action<SwaggerOptions> swaggerCallback) 
        {
            if (dataSourceCallback == null)
            {
                throw new ArgumentNullException(nameof(dataSourceCallback));
            }
            
            if (swaggerCallback == null) 
            {
                throw new ArgumentNullException(nameof(swaggerCallback));
            }
            
            var dataSourceOptions = new DataSourceOptions();
            var swaggerOptions = new SwaggerOptions();
            dataSourceCallback(dataSourceOptions);
            swaggerCallback(swaggerOptions);
            serviceCollection.AddSimpleIdentityServer(
                dataSourceOptions,
                swaggerOptions);
        }
        
        public static void AddSimpleIdentityServer(
            this IServiceCollection serviceCollection, 
            DataSourceOptions dataSourceOptions,
            SwaggerOptions swaggerOptions) 
        {
            if (dataSourceOptions == null) {
                throw new ArgumentNullException(nameof(dataSourceOptions));
            }            
            
            if (swaggerOptions == null) {
                throw new ArgumentNullException(nameof(swaggerOptions));
            }

            if (dataSourceOptions.DataSourceType == DataSourceTypes.SqlServer)
            {
                serviceCollection.AddSimpleIdentityServerSqlServer(dataSourceOptions.ConnectionString);
            }

            if (dataSourceOptions.DataSourceType == DataSourceTypes.SqlLite)
            {
                serviceCollection.AddSimpleIdentityServerSqlLite(dataSourceOptions.ConnectionString);
            }

            if (dataSourceOptions.DataSourceType == DataSourceTypes.Postgre)
            {
                serviceCollection.AddSimpleIdentityServerPostgre(dataSourceOptions.ConnectionString);
            }
            
            ConfigureSimpleIdentityServer(serviceCollection, swaggerOptions);
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
            SwaggerOptions swaggerOptions) 
        {
            services.AddSimpleIdentityServerCore();
            services.AddSimpleIdentityServerJwt();
            services.AddRateLimitation();

            services.AddTransient<ICertificateStore, CertificateStore>();
            services.AddTransient<IResourceOwnerService, InMemoryUserService>();
            services.AddTransient<IRedirectInstructionParser, RedirectInstructionParser>();
            services.AddTransient<IActionResultParser, ActionResultParser>();
            services.AddTransient<ISimpleIdentityServerConfigurator, ConcreteSimpleIdentityServerConfigurator>();
            services.AddDataProtection();
            services.AddSingleton<SwaggerOptions>(swaggerOptions);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(
                        typeof(AuthenticateController).GetTypeInfo().Assembly,
                        "SimpleIdentityServer.Host"
                ));
            });

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
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile("log-{Date}.txt")
                .Filter.ByIncludingOnly(serilogFilter)
                .CreateLogger();
            Log.Logger = logger;
            services.AddLogging();
            services.AddTransient<ISimpleIdentityServerEventSource, SimpleIdentityServerEventSource>();
            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<ISimpleIdServerConfigurationClientFactory>(new SimpleIdServerConfigurationClientFactory());
            services.AddSingleton<IIdentityServerClientFactory>(new IdentityServerClientFactory());
            services.AddTransient<IAuthenticationManager, AuthenticationManager>();
        }
        
        #endregion
    }
}