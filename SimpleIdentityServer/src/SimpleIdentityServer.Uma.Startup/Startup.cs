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
using SimpleIdentityServer.Uma.Host.Configurations;
using SimpleIdentityServer.Uma.Host.Extensions;

namespace SimpleIdentityServer.Uma.Startup
{
    public class Startup
    {
        private UmaHostConfiguration _umaHostConfiguration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _umaHostConfiguration = new UmaHostConfiguration // TH : The settings must come from the appsettings.json.
            {
                AuthorizationServer = new OauthOptions
                {
                    ClientId = "uma",
                    ClientSecret = "uma",
                    IntrospectionEndpoints = ""
                },
                DataSource = new DataSourceOptions
                {
                    IsOauthMigrated = true,
                    IsUmaMigrated = true,
                    EvtStoreDataSourceType = DbTypes.SQLSERVER,
                    OauthDbType = DbTypes.SQLSERVER,
                    UmaDbType = DbTypes.SQLSERVER,
                    UmaConnectionString = "Data Source=.;Initial Catalog=SimpleIdServerUma;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                    EvtStoreConnectionString = "Data Source=.;Initial Catalog=EventStore;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                    OauthConnectionString = "Data Source=.;Initial Catalog=SimpleIdServerOauthUma;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
                },
                Elasticsearch = new ElasticsearchOptions
                {
                    IsEnabled = false
                },
                FileLog = new FileLogsOptions
                {
                    IsEnabled = true,
                    PathFormat = "log-{Date}.txt"
                },
                Storage = new CachingOptions
                {
                    Type = CachingTypes.REDIS,
                    ConnectionString = "localhost",
                    InstanceName = "UmaInstance",
                    Port = 6379
                },
                ResourceCaching = new CachingOptions
                {
                    Type = CachingTypes.REDIS,
                    ConnectionString = "localhost",
                    InstanceName = "UmaInstance",
                    Port = 6379
                }
            };
        }

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUmaHost(_umaHostConfiguration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseUmaHost(loggerFactory, _umaHostConfiguration);
        }
    }
}
