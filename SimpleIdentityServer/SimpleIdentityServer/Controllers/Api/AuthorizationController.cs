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

            var encryptedRequest = _protector.Encrypt(authorizationRequest);
            var encodedRequest = _encoder.Encode(encryptedRequest);
            var authenticatedUser = this.GetAuthenticatedUser();
            var actionResult = _authorizationActions.GetAuthorization(
                authorizationRequest.ToParameter(), 
                authenticatedUser,
                encodedRequest);
            var parameters = _actionResultParser.GetRedirectionParameters(actionResult);
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var redirectUrl = new Uri(authorizationRequest.redirect_uri);
                var redirectUrlWithAuthCode = redirectUrl.AddParametersInQuery(parameters);
                return CreateMoveHttpResponse(redirectUrlWithAuthCode.ToString());
            }

            if (actionResult.Type == TypeActionResult.RedirectToAction)
            {
                var url = GetRedirectionUrl(Request, actionResult.RedirectInstruction.Action);
                var uri = new Uri(url);
                var redirectionUrl = uri.AddParametersInQuery(parameters);
                return CreateMoveHttpResponse(redirectionUrl.ToString());
            }

            return null;
        }

        private static string GetRedirectionUrl(
            HttpRequestMessage request,
            IdentityServerEndPoints identityServerEndPoints)
        {
            var uri = request.RequestUri.GetLeftPart(UriPartial.Authority);
            var partialUri = Constants.MappingIdentityServerEndPointToPartialUrl[identityServerEndPoints];
            return uri + partialUri;
        }

        private HttpResponseMessage CreateMoveHttpResponse(string url)
        {
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(url);
            return response;
        }
    }
}