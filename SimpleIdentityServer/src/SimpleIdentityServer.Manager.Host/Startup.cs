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
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.SwaggerGen;
using SimpleIdentityServer.Manager.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Manager.Host.Middleware;
using SimpleIdentityServer.Manager.Host.Hal;
using SimpleIdentityServer.DataAccess.SqlServer;
using System.Collections.Generic;
using SimpleIdentityServer.UserInformation.Authentication;
using SimpleIdentityServer.Core;

namespace SimpleIdentityServer.Manager.Host
{

    public class Startup
    {
        private class AssignOauth2SecurityRequirements : IOperationFilter
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                var assignedScopes = new List<string>
                {
                    "openid",
                    "SimpleIdentityServerManager:GetClients"
                };

                var oauthRequirements = new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "oauth2", assignedScopes
                    }
                };

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                operation.Security.Add(oauthRequirements);
            }
        }

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
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var authorizationUrl = Configuration["AuthorizationUrl"];
            var tokenUrl = Configuration["TokenUrl"];

            // Add the dependencies needed to run Swagger
            services.AddSwaggerGen();
            services.ConfigureSwaggerDocument(opts => {
                opts.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Simple identity server manager",
                    TermsOfService = "None"
                });
                opts.SecurityDefinitions.Add("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = authorizationUrl,
                    TokenUrl = tokenUrl,
                    Description = "Implicit flow",
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenId" },
                        { "role" , "Get the roles" }
                    }             
                });
                opts.OperationFilter<AssignOauth2SecurityRequirements>();
            });

            // Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            // Enable SqlServer
            services.AddSimpleIdentityServerSqlServer(connectionString);
            services.AddSimpleIdentityServerCore();
            services.AddSimpleIdentityServerManagerCore();

            // Add authentication
            services.AddAuthentication();

            // Add authorization policy rules
            services.AddAuthorization(options =>
            {
                options.AddPolicy("getAllClients", policy => policy.RequireClaim("role", "administrator"));
                options.AddPolicy("getClient", policy => policy.RequireClaim("role", "administrator"));
                options.AddPolicy("deleteClient", policy => policy.RequireClaim("role", "administrator"));
                options.AddPolicy("updateClient", policy => policy.RequireClaim("role", "administrator"));
            });

            services.AddSimpleIdentityServerJwt();

            // Add the dependencies needed to run MVC
            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new JsonHalMediaTypeFormatter());
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var userInfoUrl = Configuration["UserInfoUrl"];
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Display status code page
            app.UseStatusCodePages();

            // Enable CORS
            app.UseCors("AllowAll");

            // Enable custom exception handler
            app.UseSimpleIdentityServerManagerExceptionHandler();

            var userInformationOptions = new UserInformationOptions
            {
                UserInformationEndPoint = userInfoUrl
            };
            app.UseAuthenticationWithUserInformation(userInformationOptions);

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
    }
}
