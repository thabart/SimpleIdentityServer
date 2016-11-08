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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Authentication.Middleware;
using SimpleIdentityServer.Authentication.Middleware.Extensions;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Core.TwoFactors;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.Startup.TwoFactors;
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

            // 1. Configure the caching
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

            // 2. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            // 3. Configure the rate limitation
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

            // 4. Configure Simple identity server
            services.AddSimpleIdentityServer(new DataSourceOptions
            {
                DataSourceType = dataSourceType,
                ConnectionString = connectionString
            }, loggingOptions, _configurationEdpOptions.ConfigurationUrl);
            // 5. Enable logging
            services.AddLogging();
            // 6. Configure MVC
            services.AddMvc();
            // 7. Add authentication dependencies & configure it.
            services.AddAuthenticationMiddleware();
            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("Connected", policy => policy.RequireAssertion((ctx) => {
                    return ctx.User.Identity != null && ctx.User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme;
                }));
            });
            // 8. Configure two factors authentication
            var twoFactorServiceStore = new TwoFactorServiceStore();
            var factory = new SimpleIdServerConfigurationClientFactory();
            twoFactorServiceStore.Add(new TwilioSmsService(factory, _configurationEdpOptions.ConfigurationUrl));
            twoFactorServiceStore.Add(new EmailService(factory, _configurationEdpOptions.ConfigurationUrl));
            services.AddSingleton<ITwoFactorServiceStore>(twoFactorServiceStore);
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            var isDataMigrated = Configuration["DATA_MIGRATED"] == null ? false : bool.Parse(Configuration["DATA_MIGRATED"]);
            var authenticationOptions = new AuthenticationMiddlewareOptions
            {
                IdServer = new IdServerOptions
                {
                    ExternalLoginCallback = "/Authenticate/LoginCallback",
                    LoginUrls = new List<string>
                    {
                        "/Authenticate",
                        "/Authenticate/ExternalLogin",
                        "/Authenticate/OpenId",
                        "/Authenticate/LocalLoginOpenId",
                        "/Authenticate/LocalLogin",
                        "/Authenticate/ExternalLoginOpenId"
                    }
                },
                ConfigurationEdp = _configurationEdpOptions
            };

            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles();
            // 3. Redirect error to custom pages.
            app.UseStatusCodePagesWithRedirects("~/Error/{0}");
            // 4. Enable cookie authentication.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = Host.Constants.TwoFactorCookieName
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = Authentication.Middleware.Constants.CookieName
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Authenticate")
            });
            // 5. Enable multi parties authentication.
            app.UseAuthentication(authenticationOptions);
            // 6. Enable SimpleIdentityServer
            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDataMigrated = isDataMigrated,
                IsDeveloperModeEnabled = false
            }, loggerFactory);
            // 7. Configure ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute("Error401Route",
                    Host.Constants.EndPoints.Get401,
                    new
                    {
                        controller = "Error",
                        action = "Get401"
                    });
                routes.MapRoute("Error404Route",
                    Host.Constants.EndPoints.Get404,
                    new
                    {
                        controller = "Error",
                        action = "Get404"
                    });
                routes.MapRoute("Error500Route",
                    Host.Constants.EndPoints.Get500,
                    new
                    {
                        controller = "Error",
                        action = "Get500"
                    });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        #endregion
    }
}
