using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using System;

namespace SimpleIdentityServer.AccessToken.Store.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryAccessTokenStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IAccessTokenStore, InMemoryTokenStore>();
            serviceCollection.AddIdServerClient();
            return serviceCollection;
        }
    }
}
