using System;
using System.Threading.Tasks;

namespace WebApiContrib.Core.Storage
{
    public interface IStorageHelper
    {
        Task<DatedRecord<T>> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T obj);
    }

    internal class StorageHelper : IStorageHelper
    {
        private readonly IStorage _storage;

        public StorageHelper(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<DatedRecord<T>> GetAsync<T>(string key)
        {
            return await _storage.TryGetValueAsync<DatedRecord<T>>(key);
        }

        public async Task SetAsync<T>(string key, T obj)
        {
            var record = new DatedRecord<T>
            {
                CreateDate = DateTime.UtcNow,
                Obj = obj
            };
            await _storage.SetAsync(key, record);
        }
    }
}
