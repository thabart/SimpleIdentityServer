using System;
using System.Net;
using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Api.Parsers;
using SimpleIdentityServer.Core.Api.Authorization;
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
        private readonly IAuthorizationActions _authorizationActions;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly IActionResultParser _actionResultParser;

        public AuthorizationController(
            IAuthorizationActions authorizationActions,
            IProtector protector,
            IEncoder encoder,
            IActionResultParser actionResultParser)
        {
            _authorizationActions = authorizationActions;
            _protector = protector;
            _encoder = encoder;
            _actionResultParser = actionResultParser;
        }

        public HttpResponseMessage Get([FromUri]AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            var authenticatedUser = this.GetAuthenticatedUser();
            var actionResult = _authorizationActions.GetAuthorization(
                authorizationRequest.ToParameter(), 
                authenticatedUser);
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var parameters = _actionResultParser.GetRedirectionParameters(actionResult);
                var redirectUrl = new Uri(authorizationRequest.redirect_uri);
                var redirectUrlWithAuthCode = redirectUrl.AddParameters(parameters);
                return CreateMoveHttpResponse(redirectUrlWithAuthCode.ToString());
            }

            if (actionResult.Type == TypeActionResult.RedirectToAction)
            {
                var encryptedRequest = _protector.Encrypt(authorizationRequest);
                var encodedRequest = _encoder.Encode(encryptedRequest);
                var url = GetRedirectionUrl(Request, actionResult.RedirectInstruction.Action) + string.Format("?code={0}", encodedRequest);
                return CreateMoveHttpResponse(url);
            }

            return null;
        }

        private static string GetRedirectionUrl(
            HttpRequestMessage request,
            IdentityServerEndPoints identityServerEndPoints)
        {
            var uri = request.RequestUri.GetLeftPart(UriPartial.Authority);
            if (identityServerEndPoints == IdentityServerEndPoints.AuthenticateIndex)
            {
                uri = uri + "/Authenticate";
            }

            return uri;
        }

        private HttpResponseMessage CreateMoveHttpResponse(string url)
        {
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(url);
            return response;
        }
    }
}