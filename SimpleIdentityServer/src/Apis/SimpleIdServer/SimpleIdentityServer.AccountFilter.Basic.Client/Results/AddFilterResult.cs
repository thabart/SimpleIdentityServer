using SimpleIdentityServer.AccountFilter.Basic.Common.Responses;
using SimpleIdentityServer.Common.Client;

namespace SimpleIdentityServer.AccountFilter.Basic.Client.Results
{
    public class AddFilterResult : BaseResponse
    {
        public AddFilterResponse Content { get; set; }
    }
}
