#region copyright
// Copyright 2018 Habart Thierry
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
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.EF;
using WebApiContrib.Core.Storage.InMemory;
using System;
using SimpleIdentityServer.Uma.Client;
using SimpleIdentityServer.ResourceManager.API.Host.Stores;
using SimpleIdentityServer.ResourceManager.EF.InMemory;

namespace SimpleIdentityServer.ResourceManager.API.Host
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
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddMvc();
            services.AddResourceManagerInMemoryEF();
            RegisterServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAll");
            loggerFactory.AddConsole();
            app.UseStatusCodePages();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var resourceManagerContext = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>();
                try
                {
                    resourceManagerContext.Database.EnsureCreated();
                }
                catch (Exception) { }
                resourceManagerContext.EnsureSeedData();
            }
        }

        private void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddOpenIdManagerClient();
            serviceCollection.AddIdServerClient();
            serviceCollection.AddUmaClient();
            serviceCollection.AddSingleton<ITokenStore, TokenStore>();
            WebApiContrib.Core.Storage.ServiceCollectionExtensions.AddStorage(serviceCollection, opts => opts.UseInMemory());
            serviceCollection.AddSingleton<IConfiguration>(Configuration);
        }
    }
}
