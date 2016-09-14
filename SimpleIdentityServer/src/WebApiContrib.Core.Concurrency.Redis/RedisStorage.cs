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

namespace WebApiContrib.Core.Concurrency.Redis
{
    internal class RedisStorage : BaseDistributedStorage, IDisposable
    {
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

        public override IEnumerable<ConcurrentObject> GetAll()
        {
            Connect();
            var result = new List<ConcurrentObject>();
            var keys = _connection.GetServer("localhost", _port).Keys();
            foreach (var key in keys)
            {
                var results = _cache.HashMemberGet(key, "data");
                var bytes = results[0];
                var serialized = Encoding.UTF8.GetString(bytes);
                var concurentObject = JsonConvert.DeserializeObject<ConcurrentObject>(serialized);
                result.Add(concurentObject);
            }

            return result;
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
    }
}
