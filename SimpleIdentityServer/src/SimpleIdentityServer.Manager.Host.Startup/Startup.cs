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
using SimpleIdentityServer.Manager.Host.Extensions;
using Swashbuckle.SwaggerGen;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Host
{

    public class Startup
    {
        private SwaggerOptions _swaggerOptions;

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
            _swaggerOptions = new SwaggerOptions
            {
                IsEnabled = true
            };
        }

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var authorizationUrl = Configuration["AuthorizationUrl"];
            var tokenUrl = Configuration["TokenUrl"];
            services.AddSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                AuthorizationUrl = authorizationUrl,
                TokenUrl = tokenUrl
            },
            new DatabaseOptions
            {
                ConnectionString = connectionString
            }, _swaggerOptions);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var userInfoUrl = Configuration["UserInfoUrl"];
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                UserInformationUrl = userInfoUrl
            }, _swaggerOptions);
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
