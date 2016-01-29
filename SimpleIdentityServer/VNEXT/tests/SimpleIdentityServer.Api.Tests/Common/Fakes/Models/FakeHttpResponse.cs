using System.Net;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes.Models
{
    public class FakeHttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string NumberOfRequests { get; set; }

        public string NumberOfRemainingRequests { get; set; }
    }
}
