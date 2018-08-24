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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleBus.Core;
using SimpleIdentityServer.OAuth2Introspection;
using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.Db.EF.Postgre;
using SimpleIdentityServer.Scim.Host.Extensions;
using SimpleIdentityServer.Scim.Mapping.Ad;
using SimpleIdentityServer.Scim.Mapping.Ad.InMemory;
using SimpleIdentityServer.Scim.Startup.Extensions;
using SimpleIdentityServer.Scim.Startup.Services;
using SimpleIdentityServer.UserInfoIntrospection;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Scim.Startup
{

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(OAuth2IntrospectionOptions.AuthenticationScheme)
                .AddOAuth2Introspection(opts =>
                {
                    opts.ClientId = "Scim";
                    opts.ClientSecret = "~V*nH{q4;qL/=8+Z";
                    opts.WellKnownConfigurationUrl = "http://localhost:60004/.well-known/uma2-configuration";
                })
		        .AddUserInfoIntrospection(opts =>
                {
                    opts.WellKnownConfigurationUrl = "http://localhost:60000/.well-known/openid-configuration";
                });
            services.AddAuthorization(opts =>
            {
                opts.AddScimAuthPolicy();
            });
            ConfigureBus(services);
            ConfigureScimRepository(services);
            ConfigureCachingInMemory(services);
            ConfigureScimAdMappingRepository(services);
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddMvc();
            services.AddScimHost();
            services.AddScimMapping();
        }

        private void ConfigureBus(IServiceCollection services)
        {
            services.AddTransient<IEventPublisher, DefaultEventPublisher>();
            /*
            services.AddSimpleBusInMemory(new SimpleBus.Core.SimpleBusOptions
            {
                ServerName = "scim"
            });
            */
        }

        private void ConfigureScimRepository(IServiceCollection services)
        {
            services.AddScimPostgresqlEF("User ID=rocheidserver;Password=password;Host=localhost;Port=5432;Database=scim;Pooling=true;");
        }

        private void ConfigureCachingInMemory(IServiceCollection services)
        {
            services.AddConcurrency(opt => opt.UseInMemory());
        }

        private void ConfigureScimAdMappingRepository(IServiceCollection services)
        {
            services.AddScimMappingInMemoryEF();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseAuthentication();
            app.UseStatusCodePages();
            app.UseCors("AllowAll");
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scimDbContext = serviceScope.ServiceProvider.GetService<ScimDbContext>();
                scimDbContext.Database.EnsureCreated();
                scimDbContext.EnsureSeedData();
            }
        }
    }
}
