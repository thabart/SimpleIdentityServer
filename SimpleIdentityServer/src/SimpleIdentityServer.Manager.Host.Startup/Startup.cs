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
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Manager.Host.Startup
{

    public class Startup
    {
        private readonly ManagerOptions _options;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);
            var introspectionUrl = Configuration["OpenId:IntrospectUrl"];
            var clientId = Configuration["OpenId:ClientId"];
            var clientSecret = Configuration["OpenId:ClientSecret"];
            _options = new ManagerOptions
            {
                 Logging = new LoggingOptions
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
                 },
                 Introspection = new IntrospectOptions
                 {
                     IntrospectionUrl = introspectionUrl,
                     ClientId = clientId,
                     ClientSecret = clientSecret
                 }
            };
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var cachingType = Configuration["Caching:Type"];
            var databaseType = Configuration["Db:Type"];

            // 1. Configure the caching
            if (cachingType == "REDIS")
            {
                services.AddConcurrency(opt => opt.UseRedis(o =>
                {
                    o.Configuration = Configuration["Caching:ConnectionString"];
                    o.InstanceName = Configuration["Caching:InstanceName"];
                }));
            }
            else
            {
                services.AddConcurrency(opt => opt.UseInMemory());
            }
            
            // 2. Configure database
            if (databaseType == "SQLITE")
            {
                services.AddSimpleIdentityServerSqlLite(Configuration["Db:ConnectionString"]);
            }
            else if (databaseType == "POSTGRE")
            {
                services.AddSimpleIdentityServerPostgre(Configuration["Db:ConnectionString"]);
            }
            else if (databaseType == "SQLSERVER")
            {
                services.AddSimpleIdentityServerSqlServer(Configuration["Db:ConnectionString"]);
            }
            else
            {
                services.AddSimpleIdentityServerInMemory();
            }

            // 3. Configure the manager
            services.AddSimpleIdentityServerManager(_options);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            app.UseSimpleIdentityServerManager(loggerFactory, _options);
        }
    }
}
