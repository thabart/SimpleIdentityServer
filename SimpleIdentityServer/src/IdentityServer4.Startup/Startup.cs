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
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Startup.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System.Linq;
using IdentityServer4.Startup.Extensions;
using IdentityServer4.Services;
using SimpleIdentityServer.IdentityServer.EF;
using System;
using Serilog.Events;
using Serilog;
using IdentityServer4.Startup.Validation;
using IdentityServer4.Validation;

namespace IdentityServer4.Startup
{
    public class Startup
    {
        #region Properties

        public IConfigurationRoot Configuration { get; set; }

        #endregion

        #region Constructor

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        #endregion

        #region Public methods

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            services.AddIdentityServerQuickstart();
            services.AddSimpleIdentityServerSqlServer(connectionString);
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
                AuthenticationScheme = "Cookies"
            });
            app.UseCors("AllowAll");
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "Cookies",
                Authority = "http://localhost:4001/",
                RequireHttpsMetadata = false,
                ClientId = "mvc",
                ClientSecret = "secret",
                ResponseType = "code id_token",
                GetClaimsFromUserInfoEndpoint = true,
                SaveTokens = true
            });
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        #endregion

        #region Private methods

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var ctxUsers = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                if (!context.Scopes.Any())
                {
                    foreach (var s in Config.GetScopes())
                    {
                        context.Scopes.Add(s.ToEntity());
                    }

                    context.SaveChanges();
                }

                if (!context.Clients.Any())
                {
                    foreach (var c in Config.GetClients())
                    {
                        context.Clients.Add(c.ToEntity());
                    }

                    context.SaveChanges();
                }

                if (!ctxUsers.Users.Any())
                {
                    foreach(DbUser u in Config.GetUsers())
                    {
                        ctxUsers.Users.Add(u.ToEntity());
                    }

                    ctxUsers.SaveChanges();
                }
            }
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<UserLoginService>();
            services.AddTransient<IProfileService, UserProfileService>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
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
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole();
            logger.WriteTo.RollingFile("log-{Date}.txt");
            var log = logger.Filter.ByIncludingOnly(filter)
                .CreateLogger();
            Log.Logger = log;
            services.AddLogging();
        }

        #endregion
    }
}
