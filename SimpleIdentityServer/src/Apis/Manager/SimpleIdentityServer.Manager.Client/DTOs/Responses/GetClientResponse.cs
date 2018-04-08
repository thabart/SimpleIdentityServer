using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public sealed class GetClientResponse : BaseResponse
    {
        public GetClientResponse() { }

        public GetClientResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }
        
        public ClientResponse Content { get; set; }
    }
}
