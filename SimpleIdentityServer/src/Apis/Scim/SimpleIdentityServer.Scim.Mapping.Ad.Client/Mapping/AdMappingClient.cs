using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Results;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping
{
    public interface IAdMappingClient
    {
        Task<BaseResponse> AddMapping(AddMappingRequest addMappingRequest, string baseUrl, string accessToken = null);
        Task<BaseResponse> DeleteMapping(string attributeId, string baseUrl, string accessToken = null);
        Task<GetAdMappingResult> GetAdMapping(string attributeId, string baseUrl, string accessToken = null);
        Task<GetAllAdMappingResult> GetAllMappings(string baseUrl, string accessToken);
        Task<GetAdPropertiesResult> GetAllProperties(string schemaId, string baseUrl, string accessToken);
    }

    internal class AdMappingClient : IAdMappingClient
    {
        private readonly IAddAdMappingOperation _addAdMappingOperation;
        private readonly IDeleteAdMappingOperation _deleteAdMappingOperation;
        private readonly IGetAdMappingOperation _getAdMappingOperation;
        private readonly IGetAllAdMappingsOperation _getAllAdMappingsOperation;
        private readonly IGetAdPropertiesOperation _getAdPropertiesOperation;

        public AdMappingClient(IAddAdMappingOperation addAdMappingOperation, IDeleteAdMappingOperation deleteAdMappingOperation,
            IGetAdMappingOperation getAdMappingOperation, IGetAllAdMappingsOperation getAllAdMappingsOperation,
            IGetAdPropertiesOperation getAdPropertiesOperation)
        {
            _addAdMappingOperation = addAdMappingOperation;
            _deleteAdMappingOperation = deleteAdMappingOperation;
            _getAdMappingOperation = getAdMappingOperation;
            _getAllAdMappingsOperation = getAllAdMappingsOperation;
            _getAdPropertiesOperation = getAdPropertiesOperation;
        }

        public Task<BaseResponse> AddMapping(AddMappingRequest addMappingRequest, string baseUrl, string accessToken = null)
        {
            return _addAdMappingOperation.Execute(addMappingRequest, baseUrl, accessToken);
        }

        public Task<BaseResponse> DeleteMapping(string attributeId, string baseUrl, string accessToken = null)
        {
            return _deleteAdMappingOperation.Execute(attributeId, baseUrl, accessToken);
        }

        public Task<GetAdMappingResult> GetAdMapping(string attributeId, string baseUrl, string accessToken = null)
        {
            return _getAdMappingOperation.Execute(attributeId, baseUrl, accessToken);
        }

        public Task<GetAllAdMappingResult> GetAllMappings(string baseUrl, string accessToken)
        {
            return _getAllAdMappingsOperation.Execute(baseUrl, accessToken);
        }

        public Task<GetAdPropertiesResult> GetAllProperties(string schemaId, string baseUrl, string accessToken)
        {
            return _getAdPropertiesOperation.Execute(schemaId, baseUrl, accessToken);
        }
    }
}
