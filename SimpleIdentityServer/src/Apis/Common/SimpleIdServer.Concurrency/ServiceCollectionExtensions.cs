using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Storage;
using System;

namespace SimpleIdServer.Concurrency
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultConcurrency(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            
            serviceCollection.AddTransient<IRepresentationManager, DefaultRepresentationManager>();
            return serviceCollection;
        }

        public static IServiceCollection AddConcurrency(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IConcurrencyManager, ConcurrencyManager>();
            serviceCollection.AddTransient<IRepresentationManager, RepresentationManager>();
            return serviceCollection;
        }
    }
}
