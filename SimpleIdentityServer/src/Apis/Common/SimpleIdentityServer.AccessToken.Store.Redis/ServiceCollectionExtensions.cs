using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.AccessToken.Store.Redis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisTokenStore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAccessTokenStore, RedisTokenStore>();
            return services;
        }
    }
}
