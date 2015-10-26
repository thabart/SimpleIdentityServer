using System;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Operations.Authorization;
using SimpleIdentityServer.Api.Mappings;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Results;

using System.Net.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [RoutePrefix("authorization")]
    public class AuthorizationController : ApiController
    {
        private readonly IGetAuthorizationOperation _getAuthorizationOperation;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        public AuthorizationController(
            IGetAuthorizationOperation getAuthorizationOperation,
            IProtector protector,
            IEncoder encoder)
        {
            _getAuthorizationOperation = getAuthorizationOperation;
            _protector = protector;
            _encoder = encoder;
        }

        public HttpResponseMessage Get([FromUri]AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            Request.CreateResponse();
            var user = User as ClaimsPrincipal;
            
            var authorizationResult = _getAuthorizationOperation.Execute(authorizationRequest.ToParameter(), user);

            if (authorizationResult.Redirection != Redirection.No)
            {
                var encryptedRequest = _protector.Encrypt(authorizationRequest);
                var encodedRequest = _encoder.Encode(encryptedRequest);
                var url = GetRedirectionUrl(Request, authorizationResult.Redirection) + string.Format("?code={0}", encodedRequest);
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                response.Headers.Location = new Uri(url);
                return response;
            }

            return null;
        }

        private static string GetRedirectionUrl(
            HttpRequestMessage request,
            Redirection redirection)
        {
            var uri = request.RequestUri.GetLeftPart(UriPartial.Authority);
            if (redirection == Redirection.Authenticate)
            {
                uri = uri + "/Authenticate";
            }

            return uri;
        }
    }
}