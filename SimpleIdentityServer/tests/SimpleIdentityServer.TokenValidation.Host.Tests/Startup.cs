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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Uma.Authorization;
using SimpleIdentityServer.UmaIntrospection.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace SimpleIdentityServer.TokenValidation.Host.Tests
{
    public class Startup
    {
        #region Properties

        public IConfigurationRoot Configuration { get; set; }

        #endregion

        #region Public methods

        public Startup(IHostingEnvironment env)
        {
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // I. GRANT ACCESS BASED ON USER'S ROLES
            /*
            services.AddAuthorization(options =>
            {
                options.AddPolicy("getValues", policy => policy.RequireClaim("role", "administrator"));
            });
            */

            // II. GRANT ACCESS BASED ON UMA AUTHORIZATION POLICY
            services.AddAuthorization(options =>
            {
                // Add conventional uma authorization
                options.AddPolicy("uma", policy => policy.AddConventionalUma());
                // options.AddPolicy("resourceSet", policy => policy.AddResourceUma("<GUID>", "<read>","<update>"));
            });

            services.AddAuthentication();

            services.AddLogging();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseStatusCodePages();

            // I. ENABLE USER INFORMATION AUTHENTICATION
            /*
            var options = new UserInformationOptions
            {
                UserInformationEndPoint = "http://localhost:5000/userinfo"
            };
            app.UseAuthenticationWithUserInformation(options);
            */

            // II. ENABLE INTROSPECTION ENDPOINT
            /*
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "http://localhost:5000/introspect",
                ClientId = "MyBlog",
                ClientSecret = "MyBlog"
            };
            app.UseAuthenticationWithIntrospection(options);
            */

            // III. ENABLE UMA AUTHENTICATION
            var options = new UmaIntrospectionOptions
            {
                ResourcesUrl = "http://localhost:8080/api/vs/resources",
                UmaConfigurationUrl = "http://localhost:5001/.well-known/uma-configuration"
            };
            app.UseAuthenticationWithUmaIntrospection(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }

        #endregion
    }
}
