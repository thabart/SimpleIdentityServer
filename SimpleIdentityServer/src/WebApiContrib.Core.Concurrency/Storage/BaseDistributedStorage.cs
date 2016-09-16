using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApiContrib.Core.Concurrency.Storage
{
    public abstract class BaseDistributedStorage : IStorage
    {
        private IDistributedCache _distributedCache;

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

            await _distributedCache.RemoveAsync(key);
        }

        public void Set(string key, ConcurrentObject value)
        {
            SetAsync(key, value).Wait();
        }

        public async Task SetAsync(string key, ConcurrentObject value)
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
            var serializedObject = JsonConvert.SerializeObject(value);
            await _distributedCache.SetAsync(key.ToString(), Encoding.UTF8.GetBytes(serializedObject), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(300)
            });
        }

        public ConcurrentObject TryGetValue(string key)
        {
            return TryGetValueAsync(key).Result;
        }

        public async Task<ConcurrentObject> TryGetValueAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var bytes = await _distributedCache.GetAsync(key.ToString());
            if (bytes == null)
            {
                return null;
            }

            var serialized = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<ConcurrentObject>(serialized);
        }

        public abstract IEnumerable<Record> GetAll();
        
        protected void Initialize(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
    }
}
