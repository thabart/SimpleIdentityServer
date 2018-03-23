using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.EventStore.Host.Configurations;
using SimpleIdentityServer.EventStore.Host.Extensions;

namespace SimpleIdentityServer.EventStore.Startup
{

    public class Startup
    {
        private EventStoreConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _configuration = new EventStoreConfiguration
            {
                DataSource = new DataSourceConfiguration
                {
                    Type = DbTypes.SQLSERVER,
					ConnectionString = "Data Source=.;Initial Catalog=EventStore;Integrated Security=True;"
                }
            };
        }

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEventStore(_configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseEventStore(loggerFactory, _configuration);
        }
    }
}
