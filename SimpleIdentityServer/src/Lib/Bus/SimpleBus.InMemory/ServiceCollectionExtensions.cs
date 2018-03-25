using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using System;

namespace SimpleBus.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleBusInMemory(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            
            serviceCollection.AddTransient<IEventPublisher, InMemoryBus>();
            return serviceCollection;
        }
    }
}
