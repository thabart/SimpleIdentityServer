using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Uma.Core.Stores;
using System;

namespace SimpleIdentityServer.Uma.Store.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUmaInMemoryStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<ITicketStore>(new InMemoryTicketStore());
            return serviceCollection;
        }
    }
}
