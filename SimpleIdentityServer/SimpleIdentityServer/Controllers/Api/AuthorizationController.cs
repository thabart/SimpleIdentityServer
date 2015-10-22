using System;
using System.Net;
using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Core.Operations.Authorization;
using SimpleIdentityServer.Api.Mappings;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Results;

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

        public HttpResponseMessage Get([FromUri]AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            Request.CreateResponse();

            var authorizationResult = _getAuthorizationOperation.Execute(authorizationRequest.ToParameter());
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