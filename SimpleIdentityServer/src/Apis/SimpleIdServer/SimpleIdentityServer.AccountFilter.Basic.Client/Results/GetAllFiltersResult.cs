using SimpleIdentityServer.AccountFilter.Basic.Common.Responses;
using SimpleIdentityServer.Common.Client;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccountFilter.Basic.Client.Results
{
    public class GetAllFiltersResult : BaseResponse
    {
        public IEnumerable<FilterResponse> Content { get; set; }
    }
}
