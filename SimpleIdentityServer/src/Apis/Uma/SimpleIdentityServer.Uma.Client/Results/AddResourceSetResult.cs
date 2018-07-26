using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class AddResourceSetResult : BaseResponse
    {
        public AddResourceSetResponse Content { get; set; }
    }
}
