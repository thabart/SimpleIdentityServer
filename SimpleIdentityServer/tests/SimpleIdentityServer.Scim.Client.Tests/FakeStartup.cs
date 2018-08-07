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
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using SimpleIdentityServer.Scim.Client.Tests.Extensions;
using SimpleIdentityServer.Scim.Client.Tests.MiddleWares;
using SimpleIdentityServer.Scim.Client.Tests.Services;
using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.Db.EF.InMemory;
using SimpleIdentityServer.Scim.Host.Controllers;
using SimpleIdentityServer.Scim.Host.Extensions;
using System.Reflection;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Scim.Client.Tests
{
    public class FakeStartup
    {
        public const string DefaultSchema = "Cookies";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConcurrency(opt => opt.UseInMemory());
            services.AddScimInMemoryEF();
            services.AddTransient<IEventPublisher, DefaultEventPublisher>();
            /*
            services.AddSimpleBusInMemory(new SimpleBus.Core.SimpleBusOptions
            {
                ServerName = "scim"
            });
            */
            services.AddScimHost();
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            }).AddFakeCustomAuth(o => { });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("scim_manage", policy => policy.RequireAssertion((ctx) => {
                    return true;
                }));
                options.AddPolicy("scim_read", policy => policy.RequireAssertion((ctx) => {
                    return true;
                }));
                options.AddPolicy("authenticated", (policy) =>
                {
                    policy.AddAuthenticationSchemes(DefaultSchema);
                    policy.RequireAuthenticatedUser();
                });
            });
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(ResourceTypesController).GetTypeInfo().Assembly));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeDatabase(app);
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // scope.ServiceProvider.GetRequiredService<ScimDbContext>().Database.Migrate();
                var context = scope.ServiceProvider.GetRequiredService<ScimDbContext>();
                // context.Database.Migrate();
                context.EnsureSeedData();
            }
        }
    }
}
