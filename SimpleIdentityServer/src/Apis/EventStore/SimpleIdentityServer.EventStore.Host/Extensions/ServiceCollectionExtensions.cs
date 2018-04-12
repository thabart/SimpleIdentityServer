using Microsoft.AspNetCore.Builder;
using System;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder UseEventStore(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            /*
            if (loggerFactory != null)
            {
                loggerFactory.AddConsole();
            }
            */

            // 1. Display status code page.
            app.UseStatusCodePages();
            // 2. Enable CORS
            app.UseCors("AllowAll");
            // 3. Launch ASP.NET MVC
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
