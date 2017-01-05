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

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using IdentityServer4.Startup.Extensions;
using IdentityServer4.Startup.Services;
using IdentityServer4.Startup.Validation;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SimpleIdentityServer.Authentication.Middleware;
using SimpleIdentityServer.Authentication.Middleware.Extensions;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.IdentityServer.EF;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace IdentityServer4.Startup
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddIdentityServerQuickstart(opts =>
            {
                opts.IssuerUri = "https://idserver:5443";
            });
            RegisterDependencies(services);
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            InitializeDatabase(app);
            loggerFactory.AddSerilog();
            app.UseDeveloperExceptionPage();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = Constants.CookieName
            });
            app.UseAuthentication(new AuthenticationMiddlewareOptions
            {
                ConfigurationEdp = new ConfigurationEdpOptions
                {
                    ClientId = Configuration["ConfigurationEdp:ClientId"],
                    ClientSecret = Configuration["ConfigurationEdp:ClientSecret"],
                    ConfigurationUrl = Configuration["ConfigurationEdp:Url"],
                    Scopes = new List<string>
                    {
                        "configuration",
                        "display_configuration"
                    }
                },
                IdServer = new IdServerOptions
                {
                    ExternalLoginCallback = "/Account/ExternalCallback",
                    LoginUrls = new List<string>
                    {
                        "/Account/Login",
                        "/Account/External"
                    }
                }
            });

            app.UseCors("AllowAll");
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var confContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var userContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                confContext.Database.Migrate();
                userContext.Database.Migrate();
                confContext.EnsureSeedData();
                userContext.EnsureSeedData();
            }
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            var dbType = Configuration["Db:type"];
            var cachingDatabase = Configuration["Caching:Database"];
            var cachingConnectionPath = Configuration["Caching:ConnectionPath"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            Func<LogEvent, bool> filter = (e) =>
            {
                var ctx = e.Properties["SourceContext"];
                var contextValue = ctx.ToString()
                    .TrimStart('"')
                    .TrimEnd('"');
                return contextValue.StartsWith("IdentityServer") ||
                    contextValue.StartsWith("IdentityModel") ||
                    e.Level == LogEventLevel.Error ||
                    e.Level == LogEventLevel.Fatal;
            };

            // 1. Configure database
            if (string.Compare(dbType, "POSTGRES", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                services.AddSimpleIdentityServerPostGre(Configuration["Db:ConnectionString"], migrationsAssembly);
            }
            else if (string.Compare(dbType, "SQLSERVER", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                services.AddSimpleIdentityServerSqlServer(Configuration["Db:ConnectionString"], migrationsAssembly);
            }
            else
            {
                services.AddSimpleIdentityServerInMemory();
            }

            // 2. Configure caching
            if (string.Compare(cachingDatabase, "REDIS", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                services.AddStorage(opt => opt.UseRedis(o =>
                {
                    o.Configuration = Configuration["Caching:ConnectionString"];
                    o.InstanceName = Configuration["Caching:InstanceName"];
                }));
            }
            else
            {
                services.AddStorage(opt => opt.UseInMemory());
            }

            // 3. Configure the logging
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole();
            logger.WriteTo.RollingFile("log-{Date}.txt");
            var log = logger.Filter.ByIncludingOnly(filter)
                .CreateLogger();
            Log.Logger = log;
            services.AddLogging();

            services.AddAuthenticationMiddleware();
            services.AddAuthentication(opts => opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddTransient<UserLoginService>();
            services.AddTransient<IProfileService, UserProfileService>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddSingleton<Serilog.ILogger>(log);
            services.AddConfigurationClient();
            services.AddIdServerClient();
        }
    }
}
