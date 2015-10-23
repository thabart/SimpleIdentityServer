using System;
using System.Net;
using System.Security.Claims;
using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
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

        public AuthorizationController(
            IGetAuthorizationOperation getAuthorizationOperation,
            IProtector protector)
        {
            _getAuthorizationOperation = getAuthorizationOperation;
            _protector = protector;
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

            var encrypted = _protector.Encrypt(authorizationRequest);
            var decrypted = _protector.Decrypt<AuthorizationRequest>(encrypted);

            if (authorizationResult.Redirection != Redirection.No)
            {
                var url = GetRedirectionUrl(Request, authorizationResult.Redirection);
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                var uri = new UriBuilder(url)
                {
                    Query = authorizationRequest.GetQueryString()
                };
                
                response.Headers.Location = new Uri(uri.ToString());
                return response;
            }

            return null;
        }

        private static string GetRedirectionUrl(
            HttpRequestMessage request,
            Redirection redirection)
        {
            var uri = request.RequestUri.GetLeftPart(UriPartial.Authority);
            if (redirection == Redirection.Authorize)
            {
                uri = uri + "/Authorize";
            }

            return uri;
        }
    }
}