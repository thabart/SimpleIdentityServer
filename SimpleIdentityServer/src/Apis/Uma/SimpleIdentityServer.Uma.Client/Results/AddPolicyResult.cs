using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class AddPolicyResult : BaseResponse
    {
        public AddPolicyResponse Content { get; set; }
    }
}