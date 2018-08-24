using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Results
{
    public class GetAdConfigurationResult : BaseResponse
    {
        public AdConfigurationResponse Content { get; set; }
    }
}
