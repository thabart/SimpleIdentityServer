using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApiContrib.Core.Storage
{
    public abstract class BaseDistributedStorage : IStorage
    {
        private IDistributedCache _distributedCache;

        public object TryGetValue(string key)
        {
            return TryGetValueAsync(key).Result;
        }

        public async Task<object> TryGetValueAsync(string key)
        {
            var result = await GetValue(key).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject(result);
            }
            catch
            {
                return null;
            }
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
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch
            {
                return null;
            }
        }

        public void Set(string key, object value)
        {
            SetAsync(key, value).Wait();
        }

        public async Task SetAsync(string key, object value)
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
            }).ConfigureAwait(false);
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

        public abstract IEnumerable<Record> GetAll();

        public abstract void RemoveAll();

        public abstract Task RemoveAllAsync();

        protected void Initialize(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        private async Task<string> GetValue(string key)
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
    }
}
