using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApiContrib.Core.Storage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(
            this IServiceCollection serviceCollection,
            Action<StorageOptionsBuilder> callback)
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
            serviceCollection.AddTransient<IStorageHelper, StorageHelper>();
            serviceCollection.AddSingleton(builder.StorageOptions);
            return serviceCollection;
        }
    }
}
