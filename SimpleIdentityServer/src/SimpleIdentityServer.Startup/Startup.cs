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
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.RateLimitation.Configuration;
using System.Collections.Generic;

namespace SimpleIdentityServer.Startup
{
    public class Startup
    {
        private SwaggerOptions _swaggerOptions;

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
            _swaggerOptions = new SwaggerOptions
            {
                IsSwaggerEnabled = false
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
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
            }, _swaggerOptions);

            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            var clientId = Configuration["ClientId"];
            var clientSecret = Configuration["ClientSecret"];
            var configurationUrl = Configuration["ConfigurationUrl"];
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);
            app.UseCors("AllowAll");
            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDataMigrated = isDataMigrated,
                IsDeveloperModeEnabled = false
            }, _swaggerOptions, new Host.AuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ConfigurationUrl = configurationUrl
            }, loggerFactory);
        }

        #endregion
    }
}
