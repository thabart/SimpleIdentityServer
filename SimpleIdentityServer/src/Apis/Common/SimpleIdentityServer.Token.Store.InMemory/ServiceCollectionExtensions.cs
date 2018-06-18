using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using System;

namespace SimpleIdentityServer.Token.Store.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryTokenStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<ITokenStore, InMemoryTokenStore>();
            serviceCollection.AddIdServerClient();
            return serviceCollection;
        }
    }
}
