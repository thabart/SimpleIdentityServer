using System.Net;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string NumberOfRequests { get; set; }

        public string NumberOfRemainingRequests { get; set; }
    }
}
