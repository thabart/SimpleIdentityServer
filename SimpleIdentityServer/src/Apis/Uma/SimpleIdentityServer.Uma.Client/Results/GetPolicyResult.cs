using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class GetPolicyResult : BaseResponse
    {
        public PolicyResponse Content { get; set; }
    }
}
