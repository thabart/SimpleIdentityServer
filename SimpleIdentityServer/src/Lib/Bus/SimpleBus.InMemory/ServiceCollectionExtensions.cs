using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using SimpleBus.InMemory;
using System;

namespace SimpleBus.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleBusInMemory(this IServiceCollection serviceCollection, InMemoryOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            serviceCollection.AddSingleton(options);
            serviceCollection.AddTransient<IEventPublisher, InMemoryEventPublisher>();
            return serviceCollection;
        }
    }
}