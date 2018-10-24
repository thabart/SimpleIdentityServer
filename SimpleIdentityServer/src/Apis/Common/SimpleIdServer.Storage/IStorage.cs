using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Storage
{
    public interface IStorage
    {
        object TryGetValue(string key);
        Task<object> TryGetValueAsync(string key);
        T TryGetValue<T>(string key) where T : class, new();
        Task<T> TryGetValueAsync<T>(string key) where T : class, new();
        void Set(string key, object value);
        Task SetAsync(string key, object value);
        void Remove(string key);
        Task RemoveAsync(string key);
        void RemoveAll();
        Task RemoveAllAsync();
    }
}
