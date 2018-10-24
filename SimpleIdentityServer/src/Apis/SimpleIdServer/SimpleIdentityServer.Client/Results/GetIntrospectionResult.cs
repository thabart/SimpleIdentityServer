using SimpleIdentityServer.Core.Common.DTOs.Responses;

namespace SimpleIdentityServer.Client.Results
{
    public class GetIntrospectionResult : BaseSidResult
    {
        public IntrospectionResponse Content { get; set; }
    }
}
