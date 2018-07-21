using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiContrib.Core.Storage;

namespace WebApiContrib.Core.Concurrency
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
        IEnumerable<Record> GetRepresentations();
        void RemoveAll();
        Task RemoveAllAsync();
    }

    internal class ConcurrencyManager : IConcurrencyManager
    {
        private readonly StorageOptions _options;

        public ConcurrencyManager(
            StorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public ConcurrentObject TryUpdateRepresentation(string representationId)
        {
            return TryUpdateRepresentationAsync(representationId).Result;
        }

        public async Task<ConcurrentObject> TryUpdateRepresentationAsync(string representationId)
        {
            return await TryUpdateRepresentationAsync(representationId, Guid.NewGuid().ToString()).ConfigureAwait(false);
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
            await _options.Storage.SetAsync(representationId, concurrentObject).ConfigureAwait(false);
            return concurrentObject;
        }

        public IEnumerable<Record> GetRepresentations()
        {
            return _options.Storage.GetAll();
        }

        public async Task<bool> IsRepresentationDifferentAsync(string representationId, string etag)
        {
            var representation = await TryGetRepresentationAsync(representationId).ConfigureAwait(false);
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

            var value = await _options.Storage.TryGetValueAsync<ConcurrentObject>(representationId).ConfigureAwait(false);
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
            _options.Storage.RemoveAll();
        }

        public async Task RemoveAllAsync()
        {
            await _options.Storage.RemoveAllAsync().ConfigureAwait(false);
        }

        public async Task RemoveAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            await _options.Storage.RemoveAsync(name).ConfigureAwait(false);
        }
    }
}
