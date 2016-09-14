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
using SimpleIdentityServer.Configuration.Startup.Middleware;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Oauth2Instrospection.Authentication;
using System;
using WebApiContrib.Core.Concurrency.Extensions;
using WebApiContrib.Core.Concurrency.Redis;

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
            // Add the dependencies needed to run Swagger
            RegisterServices(services);

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("display", policy => policy.RequireClaim("scope", "display_configuration"));
                options.AddPolicy("manage", policy => policy.RequireClaim("scope", "manage_configuration"));
            });

            // Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            // Add authentication
            // services.AddAuthentication();

            // Add the dependencies needed to run MVC
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var introspectionUrl = Configuration["IntrospectionUrl"];
            var clientId = Configuration["ClientId"];
            var clientSecret = Configuration["ClientSecret"];
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);

            // Ensure data are inserted
            if (isDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerConfigurationContext>();
                    simpleIdentityServerContext.Database.EnsureCreated();
                    simpleIdentityServerContext.EnsureSeedData();
                }
            }

            // Enable serilog
            loggerFactory.AddSerilog();

            // Display status code page
            app.UseStatusCodePages();

            // Enable CORS
            app.UseCors("AllowAll");

            // Enable custom exception handler
            app.UseSimpleIdentityServerManagerExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                ConfigurationEventSource = app.ApplicationServices.GetService<ConfigurationEventSource>()
            });

            // Enable authentication
            var introspectionOptions = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = introspectionUrl,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            app.UseAuthenticationWithIntrospection(introspectionOptions);

            // Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }

        #region Private methods

        public void RegisterServices(IServiceCollection services)
        {
            var cachingDatabase = Configuration["Caching:Database"];
            var cachingConnectionPath = Configuration["Caching:ConnectionPath"];
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            if (string.IsNullOrWhiteSpace(cachingDatabase))
            {
                cachingDatabase = "INMEMORY";
            }

            // Configure database
            services.AddSimpleIdentityServerConfiguration();
            if (isSqlServer)
            {
                services.AddSimpleIdentityServerSqlServer(connectionString);
            }
            else if (isPostgre)
            {
                services.AddSimpleIdentityServerPostgre(connectionString);
            }

            // Configure caching
            if (cachingDatabase == "REDIS")
            {
                services.AddConcurrency(opt => opt.UseRedis(o =>
                {
                    o.Configuration = Configuration[cachingConnectionPath + ":ConnectionString"];
                    o.InstanceName = Configuration[cachingConnectionPath + ":InstanceName"];
                }, int.Parse(Configuration[cachingConnectionPath + ":Port"])));
            }
            else if (cachingDatabase == "INMEMORY")
            {
                services.AddConcurrency(opt => opt.UseInMemoryStorage());
            }

            // Configure logging
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
            services.AddLogging();
            services.AddTransient<IConfigurationEventSource, ConfigurationEventSource>();
        }

        #endregion
    }
}
