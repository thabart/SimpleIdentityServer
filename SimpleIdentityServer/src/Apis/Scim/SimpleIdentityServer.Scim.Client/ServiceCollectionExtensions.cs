using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Scim.Client
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IScimClientFactory, ScimClientFactory>();
            return services;
        }
    }
}
