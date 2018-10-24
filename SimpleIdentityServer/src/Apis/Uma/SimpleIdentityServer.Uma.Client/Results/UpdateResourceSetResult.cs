using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class UpdateResourceSetResult : BaseResponse
    {
        public UpdateResourceSetResponse Content { get; set; }
    }
}
