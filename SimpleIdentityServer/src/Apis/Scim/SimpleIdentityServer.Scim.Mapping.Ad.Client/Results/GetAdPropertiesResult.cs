using SimpleIdentityServer.Common.Client;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Results
{
    public class GetAdPropertiesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}
