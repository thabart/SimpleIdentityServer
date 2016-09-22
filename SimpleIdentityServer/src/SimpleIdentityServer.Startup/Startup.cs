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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Authentication.Middleware;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.RateLimitation.Configuration;
using System.Collections.Generic;
using WebApiContrib.Core.Storage;

namespace SimpleIdentityServer.Startup
{
    public class Startup
    {
        private ConfigurationEdpOptions _configurationEdpOptions;

        #region Properties

        public IConfigurationRoot Configuration { get; set; }

        #endregion

        #region Public methods

        public Startup(IHostingEnvironment env)
        {
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _configurationEdpOptions = new ConfigurationEdpOptions
            {
                ConfigurationUrl = Configuration["ConfigurationUrl"],
                ClientId = Configuration["ClientId"],
                ClientSecret = Configuration["ClientSecret"],
                Scopes = new List<string>
                {
                    "display_configuration"
                }
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cachingDatabase = Configuration["Caching:Database"];
            var cachingConnectionPath = Configuration["Caching:ConnectionPath"];
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            var loggingOptions = new LoggingOptions
            {
                ElasticsearchOptions = new ElasticsearchOptions
                {
                    IsEnabled = bool.Parse(Configuration["Log:Elasticsearch:Enabled"]),
                    Url = Configuration["Log:Elasticsearch:Url"]
                },
                FileLogOptions = new FileLogOptions
                {
                    IsEnabled = bool.Parse(Configuration["Log:File:Enabled"]),
                    PathFormat = Configuration["Log:File:PathFormat"]
                }
            };
            if (string.IsNullOrWhiteSpace(cachingDatabase))
            {
                cachingDatabase = "INMEMORY";
            }

            // Configure the caching
            if (cachingDatabase == "REDIS")
            {
                services.AddStorage(opt => opt.UseRedis(o =>
                {
                    o.Configuration = Configuration[cachingConnectionPath + ":ConnectionString"];
                    o.InstanceName = Configuration[cachingConnectionPath + ":InstanceName"];
                }));
            }
            else if (cachingDatabase == "INMEMORY")
            {
                services.AddStorage(opt => opt.UseInMemoryStorage());
            }

            // Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];

            // Configure the rate limitation
            services.Configure<RateLimitationOptions>(opt =>
            {
                opt.IsEnabled = true;
                opt.RateLimitationElements = new List<RateLimitationElement>
                {
                    new RateLimitationElement
                    {
                        Name = "PostToken",
                        NumberOfRequests = 20,
                        SlidingTime = 2000
                    }
                };
                opt.MemoryCache = new MemoryCache(new MemoryCacheOptions());
            });
			
            var dataSourceType = DataSourceTypes.SqlServer;
            if (isSqlLite)
            {
                dataSourceType = DataSourceTypes.SqlLite;
            }
            else if(isPostgre)
            {
                dataSourceType = DataSourceTypes.Postgre;
            }

            // Configure Simple identity server
            services.AddSimpleIdentityServer(new DataSourceOptions
            {
                DataSourceType = dataSourceType,
                ConnectionString = connectionString
            }, loggingOptions, _configurationEdpOptions);

            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);
            app.UseCors("AllowAll");
            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDataMigrated = isDataMigrated,
                IsDeveloperModeEnabled = false
            }, _configurationEdpOptions, loggerFactory);
        }

        #endregion
    }
}
