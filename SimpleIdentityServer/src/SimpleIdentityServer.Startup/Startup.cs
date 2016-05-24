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
                IsSwaggerEnabled = true
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
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
			
            var dataSourceType = DataSourceTypes.InMemory;
            if (isSqlServer)
            {
                dataSourceType = DataSourceTypes.SqlServer;
            }
            else if (isSqlLite)
            {
                dataSourceType = DataSourceTypes.SqlLite;
            }

            // Configure Simple identity server
            services.AddSimpleIdentityServer(new DataSourceOptions
            {
                DataSourceType = dataSourceType,
                ConnectionString = connectionString,
                Clients = Clients.Get(),
                JsonWebKeys = JsonWebKeys.Get(),
                ResourceOwners = ResourceOwners.Get(),
                Scopes = Scopes.Get(),
                Translations = Translations.Get()
            }, _swaggerOptions);

            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            // Enable CORS
            app.UseCors("AllowAll");

            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDataMigrated = true,
                IsDeveloperModeEnabled = false,
                IsMicrosoftAuthenticationEnabled = true,
                MicrosoftClientId = Configuration["Microsoft:ClientId"],
                MicrosoftClientSecret = Configuration["Microsoft:ClientSecret"],
                IsFacebookAuthenticationEnabled = true,
                FacebookClientId = Configuration["Facebook:ClientId"],
                FacebookClientSecret = Configuration["Facebook:ClientSecret"]
            }, _swaggerOptions);
        }

        #endregion
    }
}
