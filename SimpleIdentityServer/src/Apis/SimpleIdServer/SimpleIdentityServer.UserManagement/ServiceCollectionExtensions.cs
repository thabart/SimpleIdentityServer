using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.UserManagement.Controllers;
using System;

namespace SimpleIdentityServer.UserManagement
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserManagement(this IServiceCollection services, IMvcBuilder mvcBuilder, IHostingEnvironment hosting, UserManagementOptions userManagementOptions)
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

            if (userManagementOptions == null)
            {
                throw new ArgumentNullException(nameof(userManagementOptions));
            }

            var assembly = typeof(UserController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            var compositeProvider = new CompositeFileProvider(hosting.ContentRootFileProvider, embeddedFileProvider);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(compositeProvider);
            });

            services.AddSingleton(userManagementOptions);
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
