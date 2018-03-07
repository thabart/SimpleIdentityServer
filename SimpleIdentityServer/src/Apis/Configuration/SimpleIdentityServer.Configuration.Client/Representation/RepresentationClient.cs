using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Client.Representation
{
    public interface IRepresentationClient
    {
        Task<IEnumerable<RepresentationResponse>> GetAll(string representationUrl, string authorizationHeaderValue);
        Task<bool> Delete(string representationUrl, string authorizationHeaderValue);
    }

    internal sealed class RepresentationClient : IRepresentationClient
    {
        private readonly IDeleteRepresentationsOperation _deleteRepresentationsOperation;
        private readonly IGetRepresentationsOperation _getRepresentationsOperation;

        public RepresentationClient(IDeleteRepresentationsOperation deleteRepresentationsOperation,
            IGetRepresentationsOperation getRepresentationsOperation)
        {
            _deleteRepresentationsOperation = deleteRepresentationsOperation;
            _getRepresentationsOperation = getRepresentationsOperation;
        }

        public Task<IEnumerable<RepresentationResponse>> GetAll(string representationUrl, string authorizationHeaderValue)
        {
            return _getRepresentationsOperation.Execute(representationUrl, authorizationHeaderValue);
        }

        public Task<bool> Delete(string representationUrl, string authorizationHeaderValue)
        {
            return _deleteRepresentationsOperation.Execute(representationUrl, authorizationHeaderValue);
        }
    }
}
