using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdServer.Bus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSimpleBus(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEventPublisher>(new DefaultEventPublisher());
            return services;
        }
    }
}