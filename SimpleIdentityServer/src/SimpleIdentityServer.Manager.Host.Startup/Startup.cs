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
using SimpleIdentityServer.Manager.Host.Extensions;

namespace SimpleIdentityServer.Manager.Host.Startup
{

    public class Startup
    {
        private SwaggerOptions _swaggerOptions;

        public Startup(IHostingEnvironment env)
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
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            var authorizationUrl = Configuration["AuthorizationServer"] + "/authorization";
            var tokenUrl = authorizationUrl + "/token";
            var dataSourceType = DataSourceTypes.InMemory;
            if (isSqlServer)
            {
                dataSourceType = DataSourceTypes.SqlServer;
            }

            if (isPostgre)
            {
                dataSourceType = DataSourceTypes.Postgres;
            }

            services.AddSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                AuthorizationUrl = authorizationUrl,
                TokenUrl = tokenUrl
            },
            new DatabaseOptions
            {
                ConnectionString = connectionString,
                DataSourceType = dataSourceType
            }, _swaggerOptions);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var userInfoUrl = Configuration["AuthorizationServer"] + "/userinfo";
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                UserInformationUrl = userInfoUrl
            }, _swaggerOptions);
        }
    }
}
