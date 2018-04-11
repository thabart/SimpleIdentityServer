using SimpleIdentityServer.Manager.Common.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class GetAllScopesResponse : BaseResponse
    {
        public GetAllScopesResponse() { }

        public GetAllScopesResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public IEnumerable<ScopeResponse> Content { get; set; }
    }
}
