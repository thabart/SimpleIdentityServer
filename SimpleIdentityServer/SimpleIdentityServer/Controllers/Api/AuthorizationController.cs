using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Operations.Authorization;
using SimpleIdentityServer.Api.Mappings;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using System.Net.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [RoutePrefix("authorization")]
    public class AuthorizationController : ApiController
    {
        private readonly IGetAuthorizationOperation _getAuthorizationOperation;

        public AuthorizationController(IGetAuthorizationOperation getAuthorizationOperation)
        {
            _getAuthorizationOperation = getAuthorizationOperation;
        }

        public void Get([FromUri]AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            Request.CreateResponse();

            _getAuthorizationOperation.Execute(authorizationRequest.ToParameter());
        }
    }
}