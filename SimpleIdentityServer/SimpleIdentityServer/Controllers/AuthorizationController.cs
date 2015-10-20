using SimpleIdentityServer.Core.Operations;
using System.Web.Http;

namespace SimpleIdentityServer.Api.Controllers
{
    [RoutePrefix("authorization")]
    public class AuthorizationController : ApiController
    {
        private readonly IGetAuthorizationCodeOperation _getAuthorizationCodeOperation;

        public AuthorizationController(IGetAuthorizationCodeOperation getAuthorizationCodeOperation)
        {
            _getAuthorizationCodeOperation = getAuthorizationCodeOperation;
        }
    }
}