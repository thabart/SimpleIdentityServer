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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Configuration;
using SimpleIdentityServer.Manager.Host.Extensions;
using SimpleIdentityServer.RateLimitation.Configuration;
using Swashbuckle.SwaggerGen;
using System.Collections.Generic;

namespace SimpleIdentityServer.Global.Startup
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

        #region Properties

        public IConfigurationRoot Configuration { get; set; }

        #endregion

        #region Public methods

        public Startup(IHostingEnvironment env,
            IApplicationEnvironment appEnv)
        {
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                // .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _swaggerOptions = new SwaggerOptions
            {
                IsSwaggerEnabled = true
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

            ConfigureSimpleIdentityServerApplicationBuilder(app);
        }

        #endregion

        #region Public static methods

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        #endregion

        #region Private methods

        private void ConfigureSimpleIdentityServerServiceCollection(IServiceCollection services)
        {
            // Configure rate limitation
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
            
            // Configure Simple identity server
            services.AddSimpleIdentityServer(new DataSourceOptions
            {
                DataSourceType = GetDataSourceType(),
                ConnectionString = GetConnectionString(),
                Clients = Clients.Get(),
                JsonWebKeys = JsonWebKeys.Get(),
                ResourceOwners = ResourceOwners.Get(),
                Scopes = Scopes.Get(),
                Translations = Translations.Get()
            }, _swaggerOptions);
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
                ConnectionString = GetConnectionString()
            });
        }

        private void ConfigureSimpleIdentityServerApplicationBuilder(IApplicationBuilder app)
        {
            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDataMigrated = true,
                IsDeveloperModeEnabled = false,
                IsMicrosoftAuthenticationEnabled = true,
                MicrosoftClientId = GetMicrosoftClientId(),
                MicrosoftClientSecret = GetMicrosoftClientSecret(),
                IsFacebookAuthenticationEnabled = true,
                FacebookClientId = GetFacebookClientId(),
                FacebookClientSecret = GetFacebookClientSecret()
            }, _swaggerOptions);
        }

        private DataSourceTypes GetDataSourceType()
        {
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
            var dataSourceType = DataSourceTypes.InMemory;
            if (isSqlServer)
            {
                dataSourceType = DataSourceTypes.SqlServer;
            }
            else if (isSqlLite)
            {
                dataSourceType = DataSourceTypes.SqlLite;
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
