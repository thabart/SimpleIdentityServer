using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SimpleIdentityServer.UserManagement.Controllers;
using System;

namespace SimpleIdentityServer.UserManagement
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserManagement(this IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            var assembly = typeof(UserController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
