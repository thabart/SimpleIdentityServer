using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Api.Parsers;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Results;

using System.Net.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class AuthorizationController : ApiController
    {
        private readonly IAuthorizationActions _authorizationActions;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly IActionResultParser _actionResultParser;

        private readonly IJwtParser _jwtParser;

        private readonly IHttpClientFactory _httpClientFactory;

        public AuthorizationController(
            IAuthorizationActions authorizationActions,
            IProtector protector,
            IEncoder encoder,
            IActionResultParser actionResultParser,
            IJwtParser jwtParser,
            IHttpClientFactory httpClientFactory)
        {
            _authorizationActions = authorizationActions;
            _protector = protector;
            _encoder = encoder;
            _actionResultParser = actionResultParser;
            _jwtParser = jwtParser;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> Get([FromUri]AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            authorizationRequest = await ResolveAuthorizationRequest(authorizationRequest);
            var encryptedRequest = _protector.Encrypt(authorizationRequest);
            var encodedRequest = _encoder.Encode(encryptedRequest);
            var authenticatedUser = this.GetAuthenticatedUser();
            var parameter = authorizationRequest.ToParameter();
            var actionResult = _authorizationActions.GetAuthorization(
                parameter,
                authenticatedUser,
                encodedRequest);
            var parameters = _actionResultParser.GetRedirectionParameters(actionResult);
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var redirectUrl = new Uri(authorizationRequest.redirect_uri);
                return this.CreateRedirectHttpTokenResponse(redirectUrl, parameters, parameter.ResponseMode);
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
            var uri = request.GetAbsoluteUriWithVirtualPath();
            var partialUri = Constants.MappingIdentityServerEndPointToPartialUrl[identityServerEndPoints];
            return uri + partialUri;
        }

        private HttpResponseMessage CreateMoveHttpResponse(string url)
        {
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(url);
            return response;
        }

        /// <summary>
        /// Get the correct authorization request.
        /// 1. The request parameter can contains a self-contained JWT token which contains the claims of the authorization request.
        /// 2. The request_uri can be used to download the JWT token and constructs the authorization request from it.
        /// </summary>
        /// <param name="authorizationRequest"></param>
        /// <returns></returns>
        private async Task<AuthorizationRequest> ResolveAuthorizationRequest(AuthorizationRequest authorizationRequest)
        {
            if (!string.IsNullOrWhiteSpace(authorizationRequest.request))
            {
                var result = GetAuthorizationRequestFromJwt(authorizationRequest.request);
                if (result == null)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheRequestParameterIsNotCorrect,
                        authorizationRequest.state);
                }

                return result;
            }

            if (!string.IsNullOrWhiteSpace(authorizationRequest.request_uri))
            {
                Uri uri;
                if (Uri.TryCreate(authorizationRequest.request_uri, UriKind.Absolute, out uri))
                {
                    try
                    {
                        var httpClient = new HttpClient();
                        httpClient.BaseAddress = uri;

                        var httpResult = await httpClient.GetAsync(uri.AbsoluteUri);
                        httpResult.EnsureSuccessStatusCode();
                        var request = await httpResult.Content.ReadAsStringAsync();
                        var result = GetAuthorizationRequestFromJwt(request);
                        if (result == null)
                        {
                            throw new IdentityServerExceptionWithState(
                                ErrorCodes.InvalidRequestCode,
                                ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                                authorizationRequest.state);
                        }

                        return result;
                    }
                    catch (Exception)
                    {
                        throw new IdentityServerExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                            authorizationRequest.state);
                    }
                }

                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRequestUriParameterIsNotWellFormed,
                    authorizationRequest.state);
            }

            return authorizationRequest;
        }

        private AuthorizationRequest GetAuthorizationRequestFromJwt(string jwt)
        {
            var decrypted = _jwtParser.Decrypt(jwt);
            var jwsPayload = _jwtParser.UnSign(decrypted);
            return jwsPayload == null ? null : jwsPayload.ToAuthorizationRequest();
        }
    }
}