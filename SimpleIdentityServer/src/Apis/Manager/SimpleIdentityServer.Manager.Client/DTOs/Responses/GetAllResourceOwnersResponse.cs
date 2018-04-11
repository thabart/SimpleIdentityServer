using SimpleIdentityServer.Manager.Common.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class GetAllResourceOwnersResponse : BaseResponse
    {
        public GetAllResourceOwnersResponse() { }

        public GetAllResourceOwnersResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public IEnumerable<ResourceOwnerResponse> Content { get; set; }
    }
}
