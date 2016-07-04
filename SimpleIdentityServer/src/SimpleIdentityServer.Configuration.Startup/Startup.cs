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
using SimpleIdentityServer.Configuration.Core;
using SimpleIdentityServer.Configuration.EF;
using SimpleIdentityServer.Configuration.EF.Extensions;
using SimpleIdentityServer.Configuration.Startup.Middleware;
using SimpleIdentityServer.Oauth2Instrospection.Authentication;

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
            var introspectionUrl = Configuration["AuthorizationServerUrl"] + "/introspect";
            var clientId = Configuration["ClientId"];
            var clientSecret = Configuration["ClientSecret"];
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

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

            // Display status code page
            app.UseStatusCodePages();

            // Enable CORS
            app.UseCors("AllowAll");

            // Enable custom exception handler
            app.UseSimpleIdentityServerManagerExceptionHandler();

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
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            services.AddSimpleIdentityServerConfiguration();
            if (isSqlServer)
            {
                services.AddSimpleIdentityServerSqlServer(connectionString);
            }
            else if (isPostgre)
            {
                services.AddSimpleIdentityServerPostgre(connectionString);
            }
        }

        #endregion
    }
}
