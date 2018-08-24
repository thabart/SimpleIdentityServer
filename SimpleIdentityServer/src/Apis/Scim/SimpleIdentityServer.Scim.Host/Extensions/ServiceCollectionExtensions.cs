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
				policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (claimRole == null && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(c => c.Value == "scim_maange");
                });
            });
            options.AddPolicy("scim_read", policy =>
            {
				policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (claimRole == null && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(c => c.Value == "scim_read");
                });
            });
            options.AddPolicy("authenticated", policy =>
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
            return options;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScimCore();
        }
    }
}
