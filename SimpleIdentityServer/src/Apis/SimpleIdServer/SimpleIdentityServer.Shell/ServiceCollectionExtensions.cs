using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Shell.Controllers;
using System;

namespace SimpleIdentityServer.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicShell(this IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment hosting, BasicShellOptions shellOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (hosting == null)
            {
                throw new ArgumentNullException(nameof(hosting));
            }

            if (shellOptions == null)
            {
                throw new ArgumentNullException(nameof(shellOptions));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            var compositeProvider = new CompositeFileProvider(hosting.ContentRootFileProvider, embeddedFileProvider);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(compositeProvider);
            });

            services.AddSingleton(shellOptions);
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
