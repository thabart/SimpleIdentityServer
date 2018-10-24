using SimpleIdentityServer.Core.Common.DTOs.Responses;

namespace SimpleIdentityServer.Client.Results
{
    public class GetTokenResult : BaseSidResult
    {
        public GrantedTokenResponse Content { get; set; }
    }
}
