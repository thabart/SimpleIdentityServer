using Microsoft.Extensions.Caching.Redis;
using System;

namespace WebApiContrib.Core.Concurrency.Redis
{
    public static class ConcurrencyOptionsBuilderExtensions
    {

        public static void UseRedis(
            this ConcurrencyOptionsBuilder concurrencyOptionsBuilder,
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
            this ConcurrencyOptionsBuilder concurrencyOptionsBuilder,
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

            concurrencyOptionsBuilder.ConcurrencyOptions.Storage = new RedisStorage(options, port);
        }
    }
}
