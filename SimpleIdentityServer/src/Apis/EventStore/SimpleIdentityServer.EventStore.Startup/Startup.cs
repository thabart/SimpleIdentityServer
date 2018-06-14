using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.EF.SqlServer;
using SimpleIdentityServer.EventStore.EF;
using SimpleIdentityServer.EventStore.Host.Extensions;

namespace SimpleIdentityServer.EventStore.Startup
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddEventStoreSqlServerEF("Data Source=.;Initial Catalog=EventStore;Integrated Security=True;", null);
            services.AddEventStoreHost();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseStatusCodePages();
            app.UseCors("AllowAll");
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scimDbContext = serviceScope.ServiceProvider.GetService<EventStoreContext>();
                scimDbContext.Database.EnsureCreated();
            }
        }
    }
}
