using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Storage
{
    public class InMemoryStorage : IStorage
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IList<string> _keys;

        public InMemoryStorage(IMemoryCache memoryCache)
        {
            _keys = new List<string>();
            _memoryCache = memoryCache;
        }

        public object TryGetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            object value = null;
            if (!_memoryCache.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }

        public Task<object> TryGetValueAsync(string key)
        {
            return Task.FromResult(TryGetValue(key));
        }

        public T TryGetValue<T>(string key) where T : class, new()
        {
            return TryGetValue(key) as T;
        }

        public Task<T> TryGetValueAsync<T>(string key) where T : class, new()
        {
            return Task.FromResult<T>(TryGetValue<T>(key));
        }

        public void Set(string key, object value)
        {
            _memoryCache.Set(key, value);
            if (!_keys.Contains(key))
            {
                _keys.Add(key);
            }
        }

        public Task SetAsync(string key, object value)
        {
            return Task.Factory.StartNew(() => Set(key, value));
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _memoryCache.Remove(key);
            if (!_keys.Contains(key))
            {
                _keys.Remove(key);
            }
        }

        public Task RemoveAsync(string key)
        {
            return Task.Factory.StartNew(() => Remove(key));
        }

        public void RemoveAll()
        {
            foreach(var key in _keys)
            {
                Remove(key);
            }
        }

        public Task RemoveAllAsync()
        {
            return Task.Factory.StartNew(() => RemoveAll());
        }
    }
}
