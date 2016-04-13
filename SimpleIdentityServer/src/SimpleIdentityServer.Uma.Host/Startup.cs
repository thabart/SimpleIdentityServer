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

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using SimpleIdentityServer.Oauth2Instrospection.Authentication;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.Host.Middlewares;
using Swashbuckle.SwaggerGen;

namespace SimpleIdentityServer.Uma.Host
{

    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
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
            services.AddSwaggerGen();
            services.ConfigureSwaggerDocument(opts => {
                opts.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Simple identity server manager",
                    TermsOfService = "None"
                });
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
            var introspectionUrl = Configuration["IntrospectionUrl"];
            var clientId = Configuration["ClientId"];
            var clientSecret = Configuration["ClientSecret"];
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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

            // Launch swagger
            app.UseSwaggerGen();
            app.UseSwaggerUi();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        #region Private methods

        public void RegisterServices(IServiceCollection services)
        {
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            services.AddSimpleIdServerUmaCore();
            services.AddSimpleIdServerUmaSqlServer(connectionString);
        }

        #endregion
    }
}
