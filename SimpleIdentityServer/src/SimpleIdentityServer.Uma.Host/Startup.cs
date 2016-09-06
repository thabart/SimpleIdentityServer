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
            // Add the dependencies needed to run Swagger
            RegisterServices(services);

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UmaProtection", policy => policy.RequireClaim("scope", "uma_protection"));
                options.AddPolicy("Authorization", policy => policy.RequireClaim("scope", "uma_authorization"));
            });

            // Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            // Add authentication
            services.AddAuthentication();

            // Add the dependencies needed to run MVC
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var introspectionUrl = Configuration["AuthorizationServerUrl"] + "/introspect";
            var clientId = Configuration["ClientId"];
            var clientSecret = Configuration["ClientSecret"];
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);

            loggerFactory.AddSerilog();

            // Display status code page
            app.UseStatusCodePages();

            // Enable OAUTH authentication
            var introspectionOptions = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = introspectionUrl,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            app.UseAuthenticationWithIntrospection(introspectionOptions);

            // Insert seed data
            if (isDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var simpleIdServerUmaContext = serviceScope.ServiceProvider.GetService<SimpleIdServerUmaContext>();
                    simpleIdServerUmaContext.Database.EnsureCreated();
                    simpleIdServerUmaContext.EnsureSeedData();
                }
            }

            // Enable CORS
            app.UseCors("AllowAll");

            // Display exception
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });

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
            var authorizationServerUrl = Configuration["AuthorizationServerUrl"];
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var parametersProvider = new ParametersProvider(authorizationServerUrl);
            services.AddSimpleIdServerUmaCore(opt =>
            {
                opt.AuthorizeOperation = authorizationServerUrl + "/authorization";
                opt.RegisterOperation = authorizationServerUrl + "/connect/register";
                opt.TokenOperation = authorizationServerUrl + "/token";
                opt.RptLifeTime = 3000;
                opt.TicketLifeTime = 3000;
            });

            if (isSqlServer)
            {
                services.AddSimpleIdServerUmaSqlServer(connectionString);
            }

            if (isPostgre)
            {
                services.AddSimpleIdServerUmaPostgresql(connectionString);
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
