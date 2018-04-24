using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class SearchResourceOwnerResponse : BaseResponse
    {
        public SearchResourceOwnerResponse() { }

        public SearchResourceOwnerResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public SearchResourceOwnersResponse Content { get; set; }
    }
}
