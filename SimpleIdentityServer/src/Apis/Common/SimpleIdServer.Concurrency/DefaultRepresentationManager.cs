using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SimpleIdServer.Concurrency
{
    internal sealed class DefaultRepresentationManager : IRepresentationManager
    {
        public Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, bool addHeader = true)
        {
            return Task.FromResult(0);
        }

        public Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, string etag, bool addHeader = true)
        {
            return Task.FromResult(0);
        }

        public Task UpdateHeader(Controller controller, string representationId)
        {
            return Task.FromResult(0);
        }

        public Task<bool> CheckRepresentationExistsAsync(Controller controller, string representationId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CheckRepresentationHasChangedAsync(Controller controller, string representationId)
        {
            return Task.FromResult(false);
        }

        public Task RemoveRepresentations()
        {
            return Task.FromResult(0);
        }
    }
}
