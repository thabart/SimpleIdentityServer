using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleIdentityServer.EventStore.EF;
using SimpleIdentityServer.EventStore.Host.Configurations;
using System;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder UseEventStore(this IApplicationBuilder app, ILoggerFactory loggerFactory, EventStoreConfiguration configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (loggerFactory != null)
            {
                loggerFactory.AddConsole();
            }

            // 1. Display status code page.
            app.UseStatusCodePages();
            // 2. Ensure database is created.
            if (configuration.DataSource.IsDataMigrated)
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var scimDbContext = serviceScope.ServiceProvider.GetService<EventStoreContext>();
                    try
                    {
                        scimDbContext.Database.EnsureCreated();
                    }
                    catch (Exception) { }
                }
            }

            // 3. Enable CORS
            app.UseCors("AllowAll");
            // 4. Launch ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            return app;
        }
    }
}
