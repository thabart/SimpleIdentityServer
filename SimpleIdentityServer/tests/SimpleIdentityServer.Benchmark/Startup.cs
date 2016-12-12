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
using SimpleIdentityServer.Benchmark.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Benchmark
{
    public class Startup
    {
        private IdentityServerOptions _options;
        public IConfigurationRoot Configuration { get; set; }
        
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
                IsDeveloperModeEnabled = false,
                DataSource = new DataSourceOptions
                {
                    IsDataMigrated = false,
                    DataSourceType = DataSourceTypes.SqlServer,
                    ConnectionString = "Data Source=.;Initial Catalog=SimpleIdentityServer;Integrated Security=True;"
                },
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
                Authenticate = new AuthenticateOptions
                {
                    CookieName = Constants.CookieName
                },
                Scim = new ScimOptions
                {
                    IsEnabled = true,
                    EndPoint = "http://localhost:5555/"
                }
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Configure the caching
            services.AddStorage(opt => opt.UseInMemory());
            // 2. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 3. Configure Simple identity server
            services.AddSimpleIdentityServer(_options);
            // 5. Configure MVC
            services.AddMvc();
            // 6. Add authentication dependencies & configure it.
            services.AddAuthentication(opts => opts.SignInScheme = Constants.CookieName);
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("Connected", policy => policy.RequireAssertion((ctx) => {
                    return ctx.User.Identity != null && ctx.User.Identity.AuthenticationType == Constants.CookieName;
                }));
            });
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                context.EnsureSeedData();
            }

            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles();
            // 3. Enable cookie authentication.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Authenticate"),
                AuthenticationScheme = Constants.CookieName,
                CookieName = Constants.CookieName
            });
            // 5. Enable SimpleIdentityServer
            app.UseSimpleIdentityServer(_options, loggerFactory);
            // 6. Configure ASP.NET MVC
            app.UseMvc();
        }
    }
}
