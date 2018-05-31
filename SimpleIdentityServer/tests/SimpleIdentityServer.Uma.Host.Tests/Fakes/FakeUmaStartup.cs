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
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleBus.InMemory;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.EF;
using SimpleIdentityServer.EF.InMemory;
using SimpleIdentityServer.EventStore.Handler;
using SimpleIdentityServer.EventStore.InMemory;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Store.InMemory;
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.EF.InMemory;
using SimpleIdentityServer.Uma.Host.Configuration;
using SimpleIdentityServer.Uma.Host.Configurations;
using SimpleIdentityServer.Uma.Host.Controllers;
using SimpleIdentityServer.Uma.Host.Middlewares;
using SimpleIdentityServer.Uma.Host.Tests.Extensions;
using SimpleIdentityServer.Uma.Host.Tests.Services;
using SimpleIdentityServer.Uma.Logging;
using SimpleIdentityServer.Uma.Store.InMemory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Uma.Host.Tests.Fakes
{
    public class FakeUmaStartup : IStartup
    {
        private UmaHostConfiguration _configuration;
        private SharedContext _context;

        public FakeUmaStartup(SharedContext context)
        {
            _configuration = new UmaHostConfiguration();
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 1. Add the dependencies.
            RegisterServices(services, _configuration);
            // 2. Add authorization policies.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UmaProtection", policy => policy.RequireAssertion((ctx) => {
                    return true;
                }));
                options.AddPolicy("Authorization", policy => policy.RequireAssertion((ctx) => {
                    return true;
                }));
            });
            // 3. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 4. Add authentication.
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API.
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(ConfigurationController).GetTypeInfo().Assembly));
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            // 1. Insert seed data
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var simpleIdServerUmaContext = serviceScope.ServiceProvider.GetService<SimpleIdServerUmaContext>();
                try
                {
                    simpleIdServerUmaContext.Database.EnsureCreated();
                }
                catch (Exception) { }

                simpleIdServerUmaContext.EnsureSeedData();
            }

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                simpleIdentityServerContext.Database.EnsureCreated();
                simpleIdentityServerContext.EnsureSeedData(_context);
            }

            app.Use(async (context, next) =>
            {
                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("client_id", "resource_server")
                }, "fakests");
                context.User = new ClaimsPrincipal(claimsIdentity);
                await next.Invoke();
            });

            // 3. Enable CORS
            app.UseCors("AllowAll");
            // 4. Display exception
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });
            // 5. Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }

        private static void RegisterServices(IServiceCollection services, UmaHostConfiguration configuration)
        {
            // 1. Add CORE.
            services.AddSimpleIdServerUmaCore()
                .AddSimpleIdentityServerCore()
                .AddSimpleIdentityServerJwt();

            // 2. Register DB & stores.
            services.AddUmaInMemoryEF();
            services.AddOAuthInMemoryEF();
            services.AddUmaInMemoryStore();
            services.AddInMemoryStorage();

            services.AddEventStoreInMemoryEF();
            services.AddSimpleBusInMemory()
                .AddEventStoreBusHandler(new EventStoreHandlerOptions(ServerTypes.AUTH));

            services.AddConcurrency(opt => opt.UseInMemory());

            // 3. Enable logging.
            services.AddLogging();
            services.AddIdServerLogging();
            // 4. Register the services.
            services.AddTransient<SimpleIdentityServer.Core.Services.IConfigurationService, DefaultConfigurationService>();
            services.AddTransient<SimpleIdentityServer.Core.Services.IAuthenticateResourceOwnerService, DefaultAuthenticateResourceOwnerService>();
            // 5. Register other classes.
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
            services.AddTransient<IIdentityServerClientFactory, FakeIdentityServerClientFactory>();
        }
    }
}
