using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class GetScopeResponse : BaseResponse
    {
        public GetScopeResponse() { }

        public GetScopeResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public ScopeResponse Content { get; set; }
    }
}
