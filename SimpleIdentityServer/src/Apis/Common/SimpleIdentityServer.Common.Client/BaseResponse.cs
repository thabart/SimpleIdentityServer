using SimpleIdentityServer.Common.Dtos.Responses;
using System.Net;

namespace SimpleIdentityServer.Common.Client
{
    public class BaseResponse
    {
        public HttpStatusCode HttpStatus { get; set; }
        public bool ContainsError { get; set; }
        public ErrorResponse Error { get; set; }
    }
}