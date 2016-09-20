using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApiContrib.Core.Storage
{
    public class InMemoryStorage : IStorage
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryStorage()
        {
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
        }

        public Task RemoveAsync(string key)
        {
            return Task.Factory.StartNew(() => Remove(key));
        }

        public IEnumerable<Record> GetAll()
        {
            // IMPLEMENT
            return new List<Record>();
        }

        public void RemoveAll()
        {
            // IMPLEMENT
        }

        public Task RemoveAllAsync()
        {
            // IMPLEMENT
            return Task.FromResult(0);
        }
    }
}
