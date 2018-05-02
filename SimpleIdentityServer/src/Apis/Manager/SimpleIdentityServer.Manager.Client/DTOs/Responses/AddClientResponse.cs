using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Manager.Common.Responses;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class AddClientResponse : BaseResponse
    {
        public AddClientResponse() { }

        public AddClientResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public ClientRegistrationResponse Content { get; set; }
    }
}
