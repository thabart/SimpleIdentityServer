using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

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
            /*
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            */
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            services.AddEntityFramework()
               .AddSqlServer()
               .AddDbContext<SimpleIdentityServerContext>(options =>
                   options.UseSqlServer("Data Source=W262C017\\SQLEXPRESS;Initial Catalog=SimpleIdentityServer;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"));
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
        }

        #endregion
    }
}
