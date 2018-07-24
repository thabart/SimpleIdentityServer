using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountFilter(this IServiceCollection services, AccountFilterBasicOptions options)
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
            services.AddTransient<IAccountFilter, AccountFilter>();
            return services;
        }
    }
}
