using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Results;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Configuration
{
    public interface IAdConfigurationClient
    {
        Task<GetAdConfigurationResult> GetConfiguration(string baseUrl, string accessToken = null);
        Task<BaseResponse> UpdateConfiguration(UpdateAdConfigurationRequest request, string baseUrl, string accessToken = null);
    }

    internal class AdConfigurationClient : IAdConfigurationClient
    {
        private readonly IGetAdConfigurationOperation _getAdConfigurationOperation;
        private readonly IUpdateAdConfigurationOperation _updateAdConfigurationOperation;

        public AdConfigurationClient(IGetAdConfigurationOperation getAdConfigurationOperation, IUpdateAdConfigurationOperation updateAdConfigurationOperation)
        {
            _getAdConfigurationOperation = getAdConfigurationOperation;
            _updateAdConfigurationOperation = updateAdConfigurationOperation;
        }

        public Task<GetAdConfigurationResult> GetConfiguration(string baseUrl, string accessToken = null)
        {
            return _getAdConfigurationOperation.Execute(baseUrl, accessToken);
        }

        public Task<BaseResponse> UpdateConfiguration(UpdateAdConfigurationRequest request, string baseUrl, string accessToken = null)
        {
            return _updateAdConfigurationOperation.Execute(request, baseUrl, accessToken);
        }
    }
}