using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class GetResourceSetResult : BaseResponse
    {
        public ResourceSetResponse Content { get; set; }
    }
}
