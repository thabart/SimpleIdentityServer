using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using System;

namespace SimpleBus.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleBusInMemory(this IServiceCollection serviceCollection, SimpleBusOptions simpleBusOptions, IEvtHandlerStore evtHandlerStore = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (simpleBusOptions == null)
            {
                throw new ArgumentNullException(nameof(simpleBusOptions));
            }

            evtHandlerStore = evtHandlerStore == null ? new EvtHandlerStore() : evtHandlerStore;
            serviceCollection.AddSingleton(evtHandlerStore);
            serviceCollection.AddSingleton(simpleBusOptions);
            serviceCollection.AddTransient<IEventPublisher, InMemoryBus>();
            return serviceCollection;
        }
    }
}