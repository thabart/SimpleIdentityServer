using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.UserFilter.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserFilter(this IServiceCollection services, UserFilterBasicOptions options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.AddSingleton(options);
            services.AddTransient<IResourceOwnerFilter, ResourceOwnerFilter>();
            return services;
        }
    }
}
