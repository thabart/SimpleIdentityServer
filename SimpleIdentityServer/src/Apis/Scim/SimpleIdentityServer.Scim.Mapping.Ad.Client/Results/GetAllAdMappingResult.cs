using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Results
{
    public class GetAllAdMappingResult : BaseResponse
    {
        public IEnumerable<MappingResponse> Content { get; set; }
    }
}
