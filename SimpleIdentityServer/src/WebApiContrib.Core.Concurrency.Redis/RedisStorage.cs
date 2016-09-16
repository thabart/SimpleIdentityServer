using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using WebApiContrib.Core.Concurrency.Storage;
using System.Collections.Generic;
using StackExchange.Redis;
using WebApiContrib.Core.Concurrency.Redis.Extensions;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace WebApiContrib.Core.Concurrency.Redis
{
    internal class RedisStorage : BaseDistributedStorage, IDisposable
    {
        private const long NotPresent = -1;

        private readonly RedisCacheOptions _options;

        private readonly int _port;

        private ConnectionMultiplexer _connection;

        private IDatabase _cache;

        public RedisStorage(RedisCacheOptions options, int port)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _port = port;
            var builder = new ServiceCollection();
            builder.AddSingleton<IDistributedCache>(serviceProvider => new RedisCache(Options.Create(options)));
            var provider = builder.BuildServiceProvider();
            Initialize((IDistributedCache)provider.GetService(typeof(IDistributedCache)));
            _options = options;
        }

        public override IEnumerable<Record> GetAll()
        {
            Connect();
            var result = new List<Record>();
            var keys = _connection.GetServer("localhost", _port).Keys();
            foreach (RedisKey key in keys)
            {
                var keyStr = Encoding.UTF8.GetString(key);
                var results = _cache.HashMemberGet(key, "absexp", "sldexp", "data");
                DateTimeOffset? absExpr;
                TimeSpan? sldExpr;
                MapMetadata(results, out absExpr, out sldExpr);
                var bytes = results[2];
                var serialized = Encoding.UTF8.GetString(bytes);
                var concurentObject = JsonConvert.DeserializeObject<ConcurrentObject>(serialized);
                result.Add(new Record
                {
                   Key = keyStr,
                   AbsoluteExpiration = absExpr,
                   SlidingExpiration = sldExpr,
                   Obj = concurentObject
                });
            }

            return result;
        }

        public override void RemoveAll()
        {
            Connect();
            var keys = _connection.GetServer("localhost", _port).Keys();
            foreach(RedisKey key in keys)
            {
                _cache.KeyDelete(key);
            }
        }

        public override async Task RemoveAllAsync()
        {
            Connect();
            var keys = _connection.GetServer("localhost", _port).Keys();
            foreach (RedisKey key in keys)
            {
                await _cache.KeyDeleteAsync(key);
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }

        private void Connect()
        {
            if (_connection == null)
            {
                _connection = ConnectionMultiplexer.Connect(_options.Configuration);
                _cache = _connection.GetDatabase();
            }
        }

        private static void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }
    }
}
