using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Api.Configuration;

namespace SimpleIdentityServer.Host
{
    public class Startup
    {
        private SwaggerOptions _swaggerOptions;
        
        #region Properties

        public IConfigurationRoot Configuration { get; set; }

        #endregion

        #region Public methods

        public Startup(IHostingEnvironment env,
            IApplicationEnvironment appEnv)
        {
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            _swaggerOptions = new SwaggerOptions 
            {
                IsSwaggerEnabled = true
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            // Configure Simple identity server
            services.AddSimpleIdentityServer(new DataSourceOptions {
                DataSourceType = DataSourceTypes.InMemory,
                ConnectionString = connectionString,
                Clients = Clients.Get(),
                JsonWebKeys = JsonWebKeys.Get(),
                ResourceOwners = ResourceOwners.Get(),
                Scopes = Scopes.Get(),
                Translations = Translations.Get()
            }, _swaggerOptions);
            
            services.AddLogging();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            
            app.UseSimpleIdentityServer(new HostingOptions
            {
                IsDeveloperModeEnabled = false,
                IsMicrosoftAuthenticationEnabled = true,
                MicrosoftClientId = Configuration["Microsoft:ClientId"],
                MicrosoftClientSecret = Configuration["Microsoft:ClientSecret"],
                IsFacebookAuthenticationEnabled = true,
                FacebookClientId = Configuration["Facebook:ClientId"],
                FacebookClientSecret = Configuration["Facebook:ClientSecret"]
            }, _swaggerOptions);
        }

        #endregion

        #region Public static methods

        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);

        #endregion
    }
}
