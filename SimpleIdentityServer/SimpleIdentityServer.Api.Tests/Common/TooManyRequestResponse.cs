using System.Net;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class TooManyRequestResponse
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public string Message { get; set; }

        public string NumberOfRequests { get; set; }

        public string NumberOfRemainingRequests { get; set; }
    }
}
