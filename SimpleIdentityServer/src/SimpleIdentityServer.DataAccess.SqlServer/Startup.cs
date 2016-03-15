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

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Linq;

namespace SimpleIdentityServer.DataAccess.SqlServer
{
    public class Startup
    {
        #region Properties

        public IConfigurationRoot Configuration { get; set; }

        #endregion

        #region Public methods

        public Startup(IHostingEnvironment env,
            IApplicationEnvironment appEnv)
        {
            var environment = GetEnvironment();
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            var isSqlServer = bool.Parse(Configuration["isSqlServer"]);
            var isSqlLite = bool.Parse(Configuration["isSqlLite"]);
            Console.WriteLine(isSqlServer);
            Console.WriteLine(connectionString);
            if (isSqlServer)
            {
                services.AddEntityFramework()
                   .AddSqlServer()
                   .AddDbContext<SimpleIdentityServerContext>(options =>
                       options.UseSqlServer(connectionString));
            }
            else if (isSqlLite)
            {
                services.AddEntityFramework()
                   .AddSqlite()
                   .AddDbContext<SimpleIdentityServerContext>(options =>
                       options.UseSqlite(connectionString));
            }
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
        }

        #endregion

        #region Private methods

        private static string GetEnvironment()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            var environmentVariables = configuration.Build();
            var environmentVariable = environmentVariables.GetChildren().FirstOrDefault(e => e.Key == "ASPNET_ENV");
            return environmentVariable == null ? string.Empty : environmentVariable.Value;
        }
       
        #endregion
    }
}
