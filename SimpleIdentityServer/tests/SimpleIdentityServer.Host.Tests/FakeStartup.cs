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
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Api.Controllers.Api;
using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.Host.Tests.Extensions;
using SimpleIdentityServer.Host.Tests.Services;
using System.Reflection;
using WebApiContrib.Core.Storage;
using WebApiContrib.Core.Storage.InMemory;

namespace SimpleIdentityServer.Host.Tests
{
    public class FakeStartup
    {
        public const string ScimEndPoint = "http://localhost:5555/";
        private IdentityServerOptions _options;

        public FakeStartup()
        {
            _options = new IdentityServerOptions
            {

                IsDeveloperModeEnabled = false,
                DataSource = new DataSourceOptions
                {
                    DataSourceType = DataSourceTypes.InMemory,
                    IsDataMigrated = false
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
                    CookieName = "SimpleIdServer"
                },
                Scim = new ScimOptions
                {
                    IsEnabled = true,
                    EndPoint = ScimEndPoint
                },
                AuthenticateResourceOwner = typeof(CustomAuthenticateResourceOwnerService)
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
            // 4. Enable logging
            services.AddLogging();
            // 5. Configure MVC
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(DiscoveryController).GetTypeInfo().Assembly));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var simpleIdentityServerContext = serviceScope.ServiceProvider.GetService<SimpleIdentityServerContext>();
                simpleIdentityServerContext.Database.EnsureCreated();
                simpleIdentityServerContext.EnsureSeedData();
            }

            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles();
            // 3. Use simple identity server.
            app.UseSimpleIdentityServer(_options, loggerFactory);
            // 4. Use MVC.
            app.UseMvc();
        }
    }
}
