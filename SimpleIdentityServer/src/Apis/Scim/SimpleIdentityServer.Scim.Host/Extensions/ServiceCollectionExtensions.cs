using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Core;
using System;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimHost(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            RegisterServices(services);
            return services;
        }

        public static AuthorizationOptions AddScimAuthPolicy(this AuthorizationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.AddPolicy("scim_manage", policy => policy.RequireClaim("scope", "scim_manage"));
            options.AddPolicy("scim_read", policy => policy.RequireClaim("scope", "scim_read"));
            return options;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScimCore();
        }
    }
}
