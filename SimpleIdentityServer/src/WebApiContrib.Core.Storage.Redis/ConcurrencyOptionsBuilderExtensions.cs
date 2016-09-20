using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using System;
using WebApiContrib.Core.Storage.Redis;

namespace WebApiContrib.Core.Storage
{
    public static class StorageOptionsBuilderExtensions
    {
        public static void UseRedis(
            this StorageOptionsBuilder concurrencyOptionsBuilder,
            Action<RedisCacheOptions> callback,
            int port = 6379)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var options = new RedisCacheOptions();
            callback(options);
            UseRedis(concurrencyOptionsBuilder, options, port);
        }

        public static void UseRedis(
            this StorageOptionsBuilder concurrencyOptionsBuilder,
            RedisCacheOptions options,
            int port = 6379)
        {
            if (concurrencyOptionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(concurrencyOptionsBuilder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var storage = new RedisStorage(options, port);
            concurrencyOptionsBuilder.StorageOptions.Storage = storage;
            concurrencyOptionsBuilder.ServiceCollection.AddSingleton<IStorage>(storage);
        }
    }
}
