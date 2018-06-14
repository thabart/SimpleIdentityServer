using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Shell.Controllers;
using System;

namespace SimpleIdentityServer.Shell
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseShellStaticFiles(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "SimpleIdentityServer.Shell.wwwroot");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider
            });
            return app;
        }
    }
}
