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
using SimpleIdentityServer.IdentityServer.EF;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Reflection;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.IdentityServer.Manager.Startup
{

    public class Startup
    {
        private ManagerOptions _opts;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            var isLogFileEnabled = bool.Parse(Configuration["Log:File:Enabled"]);
            var isElasticSearchEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]);
            _opts = new ManagerOptions
            {
                PasswordService = new CustomPasswordService(),
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
                    ClientId = Configuration["OpenId:ClientId"],
                    ClientSecret = Configuration["OpenId:ClientSecret"],
                    IntrospectionUrl = Configuration["OpenId:IntrospectUrl"]
                }
            };
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var databaseType = Configuration["Db:Type"];
            var cachingDatabase = Configuration["Caching:Database"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // 1. Configure the database
            if (string.Equals(databaseType, "POSTGRES", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddSimpleIdentityServerPostGre(Configuration["Db:ConnectionString"], migrationsAssembly);
            }
            else if (string.Equals(databaseType, "SQLSERVER", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddSimpleIdentityServerSqlServer(Configuration["Db:ConnectionString"], migrationsAssembly);
            }
            else
            {
                services.AddSimpleIdentityServerInMemory();
            }

            // 2. Configure the caching
            if (string.Compare(cachingDatabase, "REDIS", StringComparison.CurrentCultureIgnoreCase) == 0)
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

            // 3. Add server manager
            services.AddSimpleIdentityServerManager(_opts);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            app.UseSimpleIdentityServerManager(loggerFactory, _opts);
        }
    }
}
