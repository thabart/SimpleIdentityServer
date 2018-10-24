using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class SearchResourceSetResult : BaseResponse
    {
        public SearchResourceSetResponse Content { get; set; }
    }
}
