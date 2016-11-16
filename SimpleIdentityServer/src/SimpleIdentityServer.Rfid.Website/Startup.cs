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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Rfid.Website.Extensions;

namespace SimpleIdentityServer.Rfid.Website
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }
        private readonly IdentityServerOptions _options;

        public Startup(IHostingEnvironment env)
        {
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _options = new IdentityServerOptions
            {
                Logging = new LoggingOptions
                {
                    ElasticsearchOptions = new ElasticsearchOptions
                    {
                        IsEnabled = false
                    },
                    FileLogOptions = new FileLogOptions
                    {
                        IsEnabled = false
                    }
                },
                DataSource = new DataSourceOptions
                {
                    DataSourceType = DataSourceTypes.InMemory,
                    IsDataMigrated = false
                },
                Authenticate = new AuthenticateOptions
                {
                    CookieName = Constants.CookieName
                },
                IsDeveloperModeEnabled = false
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 2. Add the dependencies to run ASP.NET MVC
            services.AddMvc();
            // 3. Add authentication
            services.AddAuthentication();
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("Connected", policy => policy.RequireAssertion((ctx) => {
                    return ctx.User.Identity != null && ctx.User.Identity.AuthenticationType == Constants.CookieName;
                }));
            });
            // 4. Add the dependencies to run SimpleIdentityServer
            services.AddSimpleIdentityServer(_options);
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            // 1. Display status code page
            app.UseStatusCodePages();
            // 2. Enable CORS
            app.UseCors("AllowAll");
            // 3. Use static files
            app.UseStaticFiles();
            // 4. Use cookie authentication.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                SlidingExpiration = false,
                LoginPath = new PathString("/Authenticate"),
                AuthenticationScheme = Constants.CookieName,
                CookieName = Constants.CookieName
            });
            // 5. Use simpleIdentityServer
            app.UseSimpleIdentityServer(_options, loggerFactory);
            // 6. Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
            // 7. Populate the data
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                simpleIdentityServerContext.Database.EnsureCreated();
                simpleIdentityServerContext.EnsureSeedData();
            }
        }
    }
}
