using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Results
{
    public class GetAdMappingResult : BaseResponse
    {
        public MappingResponse Content { get; set; }
    }
}
