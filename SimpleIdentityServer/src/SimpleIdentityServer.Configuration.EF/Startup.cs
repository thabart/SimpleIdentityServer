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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SimpleIdentityServer.Configuration.EF
{
    public class Startup
    {
        #region Properties

        public IConfigurationRoot Configuration { get; private set; }

        #endregion

        #region Public methods

        public Startup(IHostingEnvironment env)
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
            var isPostgre = bool.Parse(Configuration["isPostgre"]);
            if (isSqlServer)
            {
                services.AddEntityFramework()
                                .AddDbContext<SimpleIdentityServerConfigurationContext>(options => options.UseSqlServer(connectionString));
            }

            else if (isPostgre)
            {
                services.AddEntityFramework()
                                .AddDbContext<SimpleIdentityServerConfigurationContext>(options => options.UseNpgsql(connectionString));
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
            var environmentVariable = environmentVariables.GetChildren().FirstOrDefault(e => e.Key == "ASPNETCORE_ENVIRONMENT");
            return environmentVariable == null ? string.Empty : environmentVariable.Value;
        }

        #endregion

        public static void Main(string[] args) { }
    }
}
