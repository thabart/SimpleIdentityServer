using SimpleIdentityServer.Core.Common.DTOs.Responses;

namespace SimpleIdentityServer.Client.Results
{
    public class GetRegisterClientResult : BaseSidResult
    {
        public ClientRegistrationResponse Content { get; set; }
    }
}
