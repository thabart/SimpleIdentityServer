using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System.Net;

namespace SimpleIdentityServer.Client.Results
{
    public class BaseSidResult
    {
        public bool ContainsError { get; set; }
        public ErrorResponseWithState Error { get; set;}
        public HttpStatusCode Status { get; set; }
    }
}
