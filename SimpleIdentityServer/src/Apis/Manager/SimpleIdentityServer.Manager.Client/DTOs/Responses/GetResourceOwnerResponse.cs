using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class GetResourceOwnerResponse : BaseResponse
    {
        public GetResourceOwnerResponse() { }

        public GetResourceOwnerResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public ResourceOwnerResponse Content { get; set; }
    }
}
