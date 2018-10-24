using System;
using System.Threading.Tasks;
using SimpleIdServer.Storage;

namespace SimpleIdServer.Concurrency
{
    public interface IConcurrencyManager
    {
        ConcurrentObject TryUpdateRepresentation(string representationId);
        Task<ConcurrentObject> TryUpdateRepresentationAsync(string representationId);
        Task<ConcurrentObject> TryUpdateRepresentationAsync(string representationId, string etag);
        Task<bool> IsRepresentationDifferentAsync(string representationId, string etag);
        ConcurrentObject TryGetRepresentation(string representationId);
        Task<ConcurrentObject> TryGetRepresentationAsync(string representationId);
        void Remove(string representationId);
        Task RemoveAsync(string representationId);
        void RemoveAll();
        Task RemoveAllAsync();
    }

    internal class ConcurrencyManager : IConcurrencyManager
    {
        private readonly IStorage _storage;
        public ConcurrencyManager(IStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            _storage = storage;
        }

        public ConcurrentObject TryUpdateRepresentation(string representationId)
        {
            return TryUpdateRepresentationAsync(representationId).Result;
        }

        public async Task<ConcurrentObject> TryUpdateRepresentationAsync(string representationId)
        {
            return await TryUpdateRepresentationAsync(representationId, Guid.NewGuid().ToString());
        }

        public async Task<ConcurrentObject> TryUpdateRepresentationAsync(string representationId, string etag)
        {
            if (string.IsNullOrWhiteSpace(representationId))
            {
                throw new ArgumentNullException(nameof(representationId));
            }

            if (string.IsNullOrWhiteSpace(etag))
            {
                throw new ArgumentNullException(nameof(etag));
            }

            var concurrentObject = new ConcurrentObject
            {
                Etag = "\"" + etag + "\"",
                DateTime = DateTime.UtcNow
            };
            await _storage.SetAsync(representationId, concurrentObject).ConfigureAwait(false);
            return concurrentObject;
        }

        public async Task<bool> IsRepresentationDifferentAsync(string representationId, string etag)
        {
            var representation = await TryGetRepresentationAsync(representationId);
            if (representation == null)
            {
                return false;
            }

            return representation.Etag.ToString() != etag;
        }

        public ConcurrentObject TryGetRepresentation(string name)
        {
            return TryGetRepresentationAsync(name).Result;
        }

        public async Task<ConcurrentObject> TryGetRepresentationAsync(string representationId)
        {
            if (string.IsNullOrWhiteSpace(representationId))
            {
                throw new ArgumentNullException(nameof(representationId));
            }

            var value = await _storage.TryGetValueAsync<ConcurrentObject>(representationId).ConfigureAwait(false);
            if (value == null)
            {
                return null;
            }

            return value;
        }

        public void Remove(string name)
        {
            RemoveAsync(name).Wait();
        }

        public void RemoveAll()
        {
            _storage.RemoveAll();
        }

        public Task RemoveAllAsync()
        {
           return  _storage.RemoveAllAsync();
        }

        public Task RemoveAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _storage.RemoveAsync(name);
        }
    }
}
