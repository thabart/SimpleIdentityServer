using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Core;
using System;
using System.Linq;

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

            options.AddPolicy("scim_manage", policy =>
            {
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScope = p.User.Claims.FirstOrDefault(c => c.Type == "scope");
                    if (claimRole == null && claimScope == null)
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" || claimScope != null && claimScope.Value == "scim_manage";
                });
            });
            options.AddPolicy("scim_read", policy =>
            {
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScope = p.User.Claims.FirstOrDefault(c => c.Type == "scope");
                    if (claimRole == null && claimScope == null)
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" || claimScope != null && claimScope.Value == "scim_read";
                });
            });
            return options;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScimCore();
        }
    }
}
