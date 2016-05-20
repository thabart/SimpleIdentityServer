using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using SimpleIdentityServer.Oauth2Instrospection.Authentication;
using SimpleIdentityServer.UserInformation.Authentication;
using SimpleIdentityServer.Uma.Authorization;
using SimpleIdentityServer.UmaIntrospection.Authentication;

namespace SimpleIdentityServer.TokenValidation.Host.Tests
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
            // Load all the configuration information from the "json" file & the environment variables.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // I. GRANT ACCESS BASED ON USER'S ROLES
            /*
            services.AddAuthorization(options =>
            {
                options.AddPolicy("getValues", policy => policy.RequireClaim("role", "administrator"));
            });
            */

            // II. GRANT ACCESS BASED ON UMA AUTHORIZATION POLICY
            services.AddAuthorization(options =>
            {
                // Add conventional uma authorization
                options.AddPolicy("uma", policy => policy.AddConventionalUma());
                // options.AddPolicy("resourceSet", policy => policy.AddResourceUma("<GUID>", "<read>","<update>"));
            });

            services.AddAuthentication();

            services.AddLogging();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseStatusCodePages();

            // I. ENABLE USER INFORMATION AUTHENTICATION
            /*
            var options = new UserInformationOptions
            {
                UserInformationEndPoint = "http://localhost:5000/userinfo"
            };
            app.UseAuthenticationWithUserInformation(options);
            */

            // II. ENABLE INTROSPECTION ENDPOINT
            /*
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "http://localhost:5000/introspect",
                ClientId = "MyBlog",
                ClientSecret = "MyBlog"
            };
            app.UseAuthenticationWithIntrospection(options);
            */

            // III. ENABLE UMA AUTHENTICATION
            var options = new UmaIntrospectionOptions
            {
                EnrichWithUmaManagerInformation = true,
                OperationUrl = "http://localhost:8080/api/operations",
                UmaConfigurationUrl = "http://localhost:5002/.well-known/uma-configuration"
            };
            app.UseAuthenticationWithUmaIntrospection(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }

        #endregion

        #region Public static methods

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        #endregion
    }
}
