using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class AddPermissionResult : BaseResponse
    {
        public AddPermissionResponse Content { get; set; }
    }
}
