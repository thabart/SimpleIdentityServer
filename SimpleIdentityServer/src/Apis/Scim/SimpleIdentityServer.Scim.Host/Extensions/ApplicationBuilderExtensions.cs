using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseScimHost(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            // 1. Enable CORS
            app.UseCors("AllowAll");
            // 2. Launch ASP.NET MVC
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
