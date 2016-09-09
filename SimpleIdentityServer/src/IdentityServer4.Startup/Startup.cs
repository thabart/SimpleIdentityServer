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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;

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
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddIdentityServerQuickstart()
                .AddConfigurationStore(builder => builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(builder => builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly)));
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            InitializeDatabase(app);
            loggerFactory.AddConsole(LogLevel.Debug);
            app.UseIdentityServer();
        }

        #endregion

        #region Private methods

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                if (!context.Scopes.Any())
                {
                    foreach (var client in Config.GetScopes())
                    {
                        context.Scopes.Add(client.ToEntity());
                    }

                    context.SaveChanges();
                }
            }
        }

        #endregion
    }
}
