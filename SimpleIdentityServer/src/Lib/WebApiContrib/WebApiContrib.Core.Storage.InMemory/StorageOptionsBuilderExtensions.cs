using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApiContrib.Core.Storage.InMemory
{
    public static class StorageOptionsBuilderExtensions
    {
        public static void UseInMemory(this StorageOptionsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            
            var storage = new InMemoryStorage();
            builder.StorageOptions.Storage = storage;
            builder.ServiceCollection.AddSingleton<IStorage>(storage);
        }
    }
}
