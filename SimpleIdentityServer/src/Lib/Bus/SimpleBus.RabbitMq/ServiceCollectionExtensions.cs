using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using System;

namespace SimpleBus.RabbitMq
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleBusRabbitMq(this IServiceCollection serviceCollection, RabbitMqOptions options)
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
            serviceCollection.AddTransient<IEventPublisher, RabbitMqEventPublisher>();
            return serviceCollection;
        }
    }
}