using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Text.RegularExpressions;
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

            if (!IsIpAddress(options.Configuration))
            {
                IPHostEntry ip = Dns.GetHostEntryAsync(options.Configuration).Result;
                var ipAddress = string.Empty;
                foreach(var adr in ip.AddressList)
                {
                    var strIp = adr.ToString();
                    if (IsIpAddress(strIp))
                    {
                        ipAddress = strIp;
                        break;
                    }
                }

                options.Configuration = ipAddress;
            }

            var storage = new RedisStorage(options, port);
            concurrencyOptionsBuilder.StorageOptions.Storage = storage;
            concurrencyOptionsBuilder.ServiceCollection.AddSingleton<IStorage>(storage);
        }

        private static bool IsIpAddress(string host)
        {
            string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            return Regex.IsMatch(host, ipPattern);
        }
    }
}
