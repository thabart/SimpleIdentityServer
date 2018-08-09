using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.AccountFilter.Basic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountFilter(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAccountFilter, AccountFilter>();
            return services;
        }
    }
}
