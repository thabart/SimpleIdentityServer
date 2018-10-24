using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class SearchAuthPoliciesResult : BaseResponse
    {
        public SearchAuthPoliciesResponse Content { get; set; }
    }
}
