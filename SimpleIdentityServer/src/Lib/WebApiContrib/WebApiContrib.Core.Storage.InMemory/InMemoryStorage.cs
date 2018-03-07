using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiContrib.Core.Storage.InMemory
{
    public class InMemoryStorage : IStorage
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IList<string> _keys;

        public InMemoryStorage()
        {
            _keys = new List<string>();
            var builder = new ServiceCollection();
            builder.AddMemoryCache();
            var provider = builder.BuildServiceProvider();
            _memoryCache = (IMemoryCache)provider.GetService(typeof(IMemoryCache));
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

        public IEnumerable<Record> GetAll()
        {
            var result = new List<Record>();
            foreach(var key in _keys)
            {
                var value = TryGetValue<Record>(key);
                if (value != null)
                {
                    result.Add(value);
                }
            }

            return result;
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
