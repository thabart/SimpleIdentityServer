using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.Authenticate.Basic.Controllers;
using System;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment hosting, BasicAuthenticateOptions options)
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

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            var compositeProvider = new CompositeFileProvider(hosting.ContentRootFileProvider, embeddedFileProvider);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(compositeProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
            services.AddSingleton(options);
            return services;
        }
    }
}
