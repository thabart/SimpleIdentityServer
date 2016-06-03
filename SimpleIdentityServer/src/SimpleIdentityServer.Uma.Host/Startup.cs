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
using SimpleIdentityServer.Oauth2Instrospection.Authentication;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.Host.Configuration;

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
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

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


            // Enable CORS
            app.UseCors("AllowAll");

            // Display exception
            app.UseExceptionHandler();

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
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var parametersProvider = new ParametersProvider(authorizationServerUrl);
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IParametersProvider>(parametersProvider);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
        }

        #endregion
    }
}
