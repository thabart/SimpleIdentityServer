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
using SimpleBus.InMemory;
using SimpleIdentityServer.EventStore.EF;
using SimpleIdentityServer.OAuth2Introspection;
using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.EventStore.Handler;
using SimpleIdentityServer.Scim.Host.Configurations;
using SimpleIdentityServer.Scim.Host.Extensions;
using SimpleIdentityServer.Scim.Startup.Extensions;

namespace SimpleIdentityServer.Scim.Startup
{

    public class Startup
    {
        private ScimConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _configuration = new ScimConfiguration
            {
                DataSource = new DbOptions
                {
                    Type = DbTypes.INMEMORY
                },
                Caching = new CachingOptions
                {
                    Type = CachingTypes.INMEMORY
                }
            };
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
                });
            services.AddEventStoreSqlServer("Data Source=.;Initial Catalog=EventStore;Integrated Security=True;").AddSimpleBusInMemory().AddEventStoreBus().AddScim(_configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
            app.UseScimHost(loggerFactory, _configuration);
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scimDbContext = serviceScope.ServiceProvider.GetService<ScimDbContext>();
                scimDbContext.EnsureSeedData();
            }
        }
    }
}
