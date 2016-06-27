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
using SimpleIdentityServer.Manager.Host.Extensions;
using SimpleIdentityServer.RateLimitation.Configuration;
using System.Collections.Generic;

using AuthorizationServer = SimpleIdentityServer.Host;
using SimpleIdentityServerManagerApi = SimpleIdentityServer.Manager.Host.Extensions;

namespace SimpleIdentityServer.Global.Startup
{
    public class Startup
    {
        private AuthorizationServer.SwaggerOptions _authorizationServerSwaggerOptions;

        private SimpleIdentityServerManagerApi.SwaggerOptions _simpleIdentityServerManagerApiSwaggerOptions;

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
            _authorizationServerSwaggerOptions = new AuthorizationServer.SwaggerOptions
            {
                IsSwaggerEnabled = false
            };
            _simpleIdentityServerManagerApiSwaggerOptions = new SimpleIdentityServerManagerApi.SwaggerOptions
            {
                IsEnabled = false
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            ConfigureSimpleIdentityServerServiceCollection(services);
            ConfigureSimpleIdentityServerManagerServiceCollection(services);

            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            // Enable CORS
            app.UseCors("AllowAll");

            // Configure static files
            var defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);

            app.UseStaticFiles();

            app.Map("/authorization", auth =>
            {
                ConfigureSimpleIdentityServerApplicationBuilder(auth);
            });

            app.Map("/managerapi", managerApi =>
            {
                ConfigureSimpleIdentityServerManagerApplicationBuilder(managerApi);
            });  
        }

        #endregion

        #region Private methods

        private void ConfigureSimpleIdentityServerServiceCollection(IServiceCollection services)
        {
            // Configure rate limitation
            /*
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
            */

            // Configure Simple identity server
            services.AddSimpleIdentityServer(new DataSourceOptions
            {
                DataSourceType = GetAuthorizationDataSourceType(),
                ConnectionString = GetConnectionString()
            }, _authorizationServerSwaggerOptions);
        }

        private void ConfigureSimpleIdentityServerApplicationBuilder(IApplicationBuilder app)
        {
            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDataMigrated = true,
                IsDeveloperModeEnabled = false
            }, _authorizationServerSwaggerOptions, null);
        }

        private void ConfigureSimpleIdentityServerManagerServiceCollection(IServiceCollection services)
        {
            services.AddSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                AuthorizationUrl = GetAuthorizationUrl(),
                TokenUrl = GetTokenUrl()
            },
            new DatabaseOptions
            {
                ConnectionString = GetConnectionString(),
                DataSourceType = GetSimpleIdentityServerManagerApiDataSourceType()
            },
            _simpleIdentityServerManagerApiSwaggerOptions);
        }

        private void ConfigureSimpleIdentityServerManagerApplicationBuilder(IApplicationBuilder app)
        {
            app.UseSimpleIdentityServerManager(new AuthorizationServerOptions
            {
                UserInformationUrl = GetUserInformationUrl()
            },
            _simpleIdentityServerManagerApiSwaggerOptions);
        }

        private AuthorizationServer.DataSourceTypes GetAuthorizationDataSourceType()
        {
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
            var dataSourceType = AuthorizationServer.DataSourceTypes.InMemory;
            if (isSqlServer)
            {
                dataSourceType = AuthorizationServer.DataSourceTypes.SqlServer;
            }
            else if (isSqlLite)
            {
                dataSourceType = AuthorizationServer.DataSourceTypes.SqlLite;
            }

            return dataSourceType;
        }

        private SimpleIdentityServerManagerApi.DataSourceTypes GetSimpleIdentityServerManagerApiDataSourceType()
        {
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
            var dataSourceType = SimpleIdentityServerManagerApi.DataSourceTypes.InMemory;
            if (isSqlServer)
            {
                dataSourceType = SimpleIdentityServerManagerApi.DataSourceTypes.SqlServer;
            }
            else if (isSqlLite)
            {
                dataSourceType = SimpleIdentityServerManagerApi.DataSourceTypes.SqlLite;
            }

            return dataSourceType;
        }

        private string GetConnectionString()
        {
            return Configuration[Constants.ConnectionStringName];
        }

        private string GetAuthorizationUrl()
        {
            return Configuration[Constants.AuthorizationUrlName];
        }

        private string GetTokenUrl()
        {
            return Configuration[Constants.TokenUrlName];
        }

        private string GetUserInformationUrl()
        {
            return Configuration[Constants.UserInformationUrlName];
        }

        private string GetMicrosoftClientId()
        {
            return Configuration[Constants.MicrosoftClientIdName];
        }

        private string GetMicrosoftClientSecret()
        {
            return Configuration[Constants.MicrosoftClientSecret];
        }

        private string GetFacebookClientId()
        {
            return Configuration[Constants.FacebookClientIdName];
        }

        private string GetFacebookClientSecret()
        {
            return Configuration[Constants.FacebookClientSecretName];
        }

        #endregion
    }
}
