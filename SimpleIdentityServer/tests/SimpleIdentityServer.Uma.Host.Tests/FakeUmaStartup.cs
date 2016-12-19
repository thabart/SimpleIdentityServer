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
using SimpleIdentityServer.Uma.Core;
using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.Host.Configuration;
using SimpleIdentityServer.Uma.Host.Controllers;
using SimpleIdentityServer.Uma.Host.Middlewares;
using SimpleIdentityServer.Uma.Host.Tests.Extensions;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Uma.Host.Tests
{
    public class FakeUmaStartup : IStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            // 1. Display status code page
            app.UseStatusCodePages();
            // 2. Insert seed data
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var simpleIdServerUmaContext = serviceScope.ServiceProvider.GetService<SimpleIdServerUmaContext>();
                simpleIdServerUmaContext.Database.EnsureCreated();
                simpleIdServerUmaContext.EnsureSeedData();
            }
            // 3. Use fake user
            app.Use(next => async context =>
            {
                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("client_id", "client")
                }, "fakests");
                context.User = new ClaimsPrincipal(claimsIdentity);
                await next.Invoke(context);
            });
            // 4. Display exception
            app.UseUmaExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                UmaEventSource = app.ApplicationServices.GetService<IUmaServerEventSource>()
            });
            // 5. Launch ASP.NET MVC
            app.UseMvc();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies
            RegisterServices(services);
            // 2. Add authorization policies
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
            // 4. Add authentication
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(IntrospectionController).GetTypeInfo().Assembly));
            return services.BuildServiceProvider();
        }

        private void RegisterServices(IServiceCollection services)
        {
            // 1. Enable caching.
            services.AddConcurrency(opt => opt.UseInMemory());
            // 2. Enable DB store.
            services.AddSimpleIdServerUmaInMemory();
            // 3. Add uma core
            services.AddSimpleIdServerUmaCore();
            // 4. Enable logging.
            var parametersProvider = new ParametersProvider("http://localhost");
            services.AddLogging();
            services.AddTransient<IHostingProvider, HostingProvider>();
            services.AddSingleton<IParametersProvider>(parametersProvider);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
        }
    }
}
