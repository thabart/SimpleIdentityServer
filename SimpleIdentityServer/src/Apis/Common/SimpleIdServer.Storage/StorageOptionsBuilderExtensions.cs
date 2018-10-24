using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdServer.Storage
{
    public static class StorageOptionsBuilderExtensions
    {
        public static void UseInMemoryStorage(this StorageOptionsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ServiceCollection.AddMemoryCache();
            builder.ServiceCollection.AddSingleton<IStorage, InMemoryStorage>();
        }
    }
}