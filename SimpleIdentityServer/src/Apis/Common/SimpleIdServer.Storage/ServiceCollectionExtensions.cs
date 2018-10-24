using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdServer.Storage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection serviceCollection, Action<StorageOptionsBuilder> callback)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var builder = new StorageOptionsBuilder(serviceCollection);
            callback(builder);
            return serviceCollection;
        }
    }
}