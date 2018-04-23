using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Core;
using System;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScim(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 1. Add the dependencies.
            RegisterServices(services);
            // 2. Add authorization policies.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("scim_manage", policy => policy.RequireClaim("scope", "scim_manage"));
                options.AddPolicy("scim_read", policy => policy.RequireClaim("scope", "scim_read"));
            });
            // 3. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 4. Add authentication.
            services.AddAuthentication();
            // 5. Add the dependencies needed to run ASP.NET API.
            services.AddMvc();
            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScimCore();
        }
    }
}
