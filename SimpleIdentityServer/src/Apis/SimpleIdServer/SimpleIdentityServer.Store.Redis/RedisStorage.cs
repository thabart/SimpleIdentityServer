using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store.Redis
{
    internal sealed class RedisStorage
    {
        private readonly int _port;
        private readonly RedisCacheOptions _options;
        private IDistributedCache _distributedCache;

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

        public T TryGetValue<T>(string key) where T : class, new()
        {
            return TryGetValueAsync<T>(key).Result;
        }

        public async Task<T> TryGetValueAsync<T>(string key) where T : class, new()
        {
            var result = await GetValue(key).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(result, new JsonSerializerSettings
                {
                    ContractResolver = new AllPropertiesResolver()
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var bytes = await _distributedCache.GetAsync(key.ToString()).ConfigureAwait(false);
            if (bytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public void Remove(string key)
        {
            RemoveAsync(key).Wait();
        }

        public async Task RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await _distributedCache.RemoveAsync(key).ConfigureAwait(false);
        }

        public void Set(string key, object value, int slidingExpirationTime)
        {
            SetAsync(key, value, slidingExpirationTime).Wait();
        }

        public async Task SetAsync(string key, object value, int slidingExpirationTime)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var serializedObject = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesResolver()
            });
            await _distributedCache.SetAsync(key.ToString(), Encoding.UTF8.GetBytes(serializedObject), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(slidingExpirationTime)
            }).ConfigureAwait(false);
        }

        public async Task SetAsync(string key, string value, int slidingExpirationTime)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // TODO : Should be possible to configure the sliding expiration time
            await _distributedCache.SetAsync(key.ToString(), Encoding.UTF8.GetBytes(value), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(slidingExpirationTime)
            }).ConfigureAwait(false);
        }

        private void Initialize(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        private class AllPropertiesResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                //property.HasMemberAttribute = true;
                property.Ignored = false;

                //property.ShouldSerialize = instance =>
                //{
                //    return true;
                //};

                return property;
            }
        }
    }
}
