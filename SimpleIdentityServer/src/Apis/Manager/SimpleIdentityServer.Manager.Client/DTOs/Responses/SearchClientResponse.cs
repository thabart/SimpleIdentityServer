using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class SearchClientResponse : BaseResponse
    {
        public SearchClientResponse() { }

        public SearchClientResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }
        
        public SearchClientsResponse Content { get; set; }
    }
}
