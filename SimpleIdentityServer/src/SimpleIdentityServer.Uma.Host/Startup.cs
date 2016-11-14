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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using SimpleIdentityServer.Oauth2Instrospection.Authentication;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.EF.Extensions;
using SimpleIdentityServer.Uma.Host.Configuration;
using SimpleIdentityServer.Uma.Host.Middlewares;
using SimpleIdentityServer.Uma.Logging;
using System;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Uma.Host
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
            // 1. Add the dependencies
            RegisterServices(services);
            // 2. Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UmaProtection", policy => policy.RequireClaim("scope", "uma_protection"));
                options.AddPolicy("Authorization", policy => policy.RequireClaim("scope", "uma_authorization"));
            });
            // 3. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 4. Add authentication
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var introspectionUrl = Configuration["OpenId:IntrospectEndPoint"];
            var clientId = Configuration["OpenId:ClientId"];
            var clientSecret = Configuration["OpenId:ClientSecret"];
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);

            loggerFactory.AddSerilog();

            // 1. Display status code page
            app.UseStatusCodePages();
            // 2. Enable OAUTH authentication
            var introspectionOptions = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = introspectionUrl,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            app.UseAuthenticationWithIntrospection(introspectionOptions);
            // 3. Insert seed data
            if (isDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var simpleIdServerUmaContext = serviceScope.ServiceProvider.GetService<SimpleIdServerUmaContext>();
                    simpleIdServerUmaContext.Database.EnsureCreated();
                    simpleIdServerUmaContext.EnsureSeedData();
                }
            }
            // 4. Enable CORS
            app.UseCors("AllowAll");
            // 5. Display exception
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });
            // 6. Launch ASP.NET MVC
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
            var wellKnownConfiguration = Configuration["OpenId:WellKnownConfiguration"];
            var authorizationEndPoint = Configuration["OpenId:AuthorizationEndPoint"];
            var registerEndPoint = Configuration["OpenId:RegisterEndPoint"];
            var tokenEndPoint = Configuration["OpenId:TokenEndPoint"];
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            var isInMemory = bool.Parse(Configuration["isInMemory"]);
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            if (string.IsNullOrWhiteSpace(cachingDatabase))
            {
                cachingDatabase = "INMEMORY";
            }

            var parametersProvider = new ParametersProvider(wellKnownConfiguration);
            services.AddSimpleIdServerUmaCore(opt =>
            {
                opt.AuthorizeOperation = authorizationEndPoint;
                opt.RegisterOperation = registerEndPoint;
                opt.TokenOperation = tokenEndPoint;
                opt.RptLifeTime = 3000;
                opt.TicketLifeTime = 3000;
            });

            if (cachingDatabase == "REDIS")
            {
                services.AddConcurrency(opt => opt.UseRedis(o =>
                {
                    o.Configuration = Configuration[cachingConnectionPath + ":ConnectionString"];
                    o.InstanceName = Configuration[cachingConnectionPath + ":InstanceName"];
                }));
            }
            else if (cachingDatabase == "INMEMORY")
            {
                services.AddConcurrency(opt => opt.UseInMemory());
            }

            if (isSqlServer)
            {
                services.AddSimpleIdServerUmaSqlServer(connectionString);
            }

            if (isPostgre)
            {
                services.AddSimpleIdServerUmaPostgresql(connectionString);
            }

            if (isInMemory)
            {
                services.AddSimpleIdServerUmaInMemory();
            }

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
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IParametersProvider>(parametersProvider);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
        }

        #endregion
    }
}
