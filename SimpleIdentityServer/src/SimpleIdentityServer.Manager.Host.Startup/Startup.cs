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
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Manager.Host.Extensions;
using WebApiContrib.Core.Concurrency.Extensions;
using WebApiContrib.Core.Concurrency.Redis;

namespace SimpleIdentityServer.Manager.Host.Startup
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
            var cachingDatabase = Configuration["Caching:Database"];
            var cachingConnectionPath = Configuration["Caching:ConnectionPath"];
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var databaseType = Configuration["DatabaseType"];
            var authorizationUrl = Configuration["AuthorizationServer"] + "/authorization";
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);
            var tokenUrl = authorizationUrl + "/token";
            if (string.IsNullOrWhiteSpace(cachingDatabase))
            {
                cachingDatabase = "INMEMORY";
            }

            // Configure the caching
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
                services.AddConcurrency(opt => opt.UseInMemoryStorage());
            }


            // Configure database
            if (databaseType == "SQLITE")
            {
                services.AddSimpleIdentityServerSqlLite(connectionString);
            }
            else if (databaseType == "POSTGRES")
            {
                services.AddSimpleIdentityServerPostgre(connectionString);
            }
            else
            {
                services.AddSimpleIdentityServerSqlServer(connectionString);
            }

            // Configure the manager
            services.AddSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                AuthorizationUrl = authorizationUrl,
                TokenUrl = tokenUrl
            },
            new LoggingOptions
            {
                ElasticsearchOptions = new ElasticsearchOptions
                {
                    IsEnabled = isElasticSearchEnabled,
                    Url = Configuration["Log:Elasticsearch:Url"]
                },
                FileLogOptions = new FileLogOptions
                {
                    IsEnabled = isLogFileEnabled,
                    PathFormat = Configuration["Log:File:PathFormat"]
                }
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var introspectionUrl = Configuration["AuthorizationServer"] + "/introspect";
            var clientId = Configuration["ClientId"];
            var clientSecret = Configuration["ClientSecret"];
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.UseSimpleIdentityServerManager(loggerFactory, new AuthorizationServerOptions
            {
                IntrospectionUrl = introspectionUrl,
                ClientId = clientId,
                ClientSecret = clientSecret
            });
        }
    }
}
