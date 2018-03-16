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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using SimpleIdentityServer.Configuration.Core;
using SimpleIdentityServer.Configuration.EF;
using SimpleIdentityServer.Configuration.EF.Extensions;
using SimpleIdentityServer.Configuration.Host.Extensions;
using System;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Configuration.Startup
{

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.UseConfigurationService();
            RegisterServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var introspectionUrl = Configuration["IntrospectionEdp:Url"];
            var clientId = Configuration["IntrospectionEdp:ClientId"];
            var clientSecret = Configuration["IntrospectionEdp:ClientSecret"];
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);

            // 1. Ensure data are inserted
            if (isDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerConfigurationContext>();
                    try
                    {
                        simpleIdentityServerContext.Database.EnsureCreated();
                    }
                    catch (Exception) { }
                    simpleIdentityServerContext.EnsureSeedData();
                }
            }

            // 2. Enable serilog
            loggerFactory.AddSerilog();

            // 3. Enable configuration service
            app.UseConfigurationService();
        }

        private void RegisterServices(IServiceCollection services)
        {
            var cachingType = Configuration["Caching:Type"];
            var dbType = Configuration["Db:Type"];
            var connectionString = Configuration["Db:ConnectionString"];
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);

            // 1. Configure database
            services.AddSimpleIdentityServerConfiguration();
            if (string.Equals(dbType, "SQLSERVER", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddSimpleIdentityServerSqlServer(connectionString);
            }
            else if (string.Equals(dbType, "POSTGRES", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddSimpleIdentityServerPostgre(connectionString);
            } 
            else
            {
                services.AddSimpleIdentityServerInMemory();
            }

            // 2. Configure caching
            if (string.Equals(cachingType, "REDIS", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddConcurrency(opt => opt.UseRedis(o =>
                {
                    o.Configuration = Configuration["Caching:ConnectionString"];
                    o.InstanceName = Configuration["Caching:InstanceName"];
                }, int.Parse(Configuration["Caching:Port"])));
            }
            else
            {
                services.AddConcurrency(opt => opt.UseInMemory());
            }

            // 3. Configure logging
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole();
            if (isLogFileEnabled)
            {
                logger.WriteTo.RollingFile(Configuration["Log:File:PathFormat"]);
            }

            if (isElasticSearchEnabled)
            {
                logger.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Configuration["Log:Elasticsearch:Url"]))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "configuration-{0:yyyy.MM.dd}",
                    TemplateName = "configuration-events-template"
                });
            }

            Func<LogEvent, bool> serilogFilter = (e) =>
            {
                var ctx = e.Properties["SourceContext"];
                var contextValue = ctx.ToString()
                    .TrimStart('"')
                    .TrimEnd('"');
                return contextValue.StartsWith("SimpleIdentityServer.") ||
                    e.Level == LogEventLevel.Error ||
                    e.Level == LogEventLevel.Fatal;
            };
            var log = logger.Filter.ByIncludingOnly(serilogFilter)
                .CreateLogger();
            Log.Logger = log;
        }
    }
}
