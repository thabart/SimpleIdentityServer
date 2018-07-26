using SimpleIdentityServer.Common.Client;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Client.Results
{
    public class GetResourcesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}