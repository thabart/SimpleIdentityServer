using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class SearchScopeResponse : BaseResponse
    {
        public SearchScopeResponse() { }

        public SearchScopeResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }
        
        public SearchScopesResponse Content { get; set; }
    }
}
