using System;
using System.Threading.Tasks;

using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Results;

using System.Net.Http;
using SimpleIdentityServer.Core.Parameters;
using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Host;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.Authorization)]
    public class AuthorizationController : Controller
    {
        private readonly IAuthorizationActions _authorizationActions;

        private readonly IProtector _protector;

        private readonly IEncoder _encoder;

        private readonly IActionResultParser _actionResultParser;

        private readonly IJwtParser _jwtParser;

        public AuthorizationController(
            IAuthorizationActions authorizationActions,
            IProtector protector,
            IEncoder encoder,
            IActionResultParser actionResultParser,
            IJwtParser jwtParser)
        {
            _authorizationActions = authorizationActions;
            _protector = protector;
            _encoder = encoder;
            _actionResultParser = actionResultParser;
            _jwtParser = jwtParser;
        }

        #region Public methods

        [HttpGet]
        public async Task<Microsoft.AspNet.Mvc.ActionResult> Get([FromQuery]AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            authorizationRequest = await ResolveAuthorizationRequest(authorizationRequest);
            var authenticatedUser = this.GetAuthenticatedUser();
            var parameter = authorizationRequest.ToParameter();
            var actionResult = _authorizationActions.GetAuthorization(
                parameter,
                authenticatedUser);

            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var redirectUrl = new Uri(authorizationRequest.redirect_uri);
                return this.CreateRedirectHttpTokenResponse(redirectUrl,
                    _actionResultParser.GetRedirectionParameters(actionResult), 
                    actionResult.RedirectInstruction.ResponseMode);
            }

            if (actionResult.Type == TypeActionResult.RedirectToAction)
            {
                if (actionResult.RedirectInstruction.Action == IdentityServerEndPoints.AuthenticateIndex ||
                    actionResult.RedirectInstruction.Action == IdentityServerEndPoints.ConsentIndex)
                {
                    // Force the resource owner to be reauthenticated
                    if (actionResult.RedirectInstruction.Action == IdentityServerEndPoints.AuthenticateIndex)
                    {
                        authorizationRequest.prompt = Enum.GetName(typeof(PromptParameter), PromptParameter.login);
                    }

                    // Add the encoded request into the query string
                    var encryptedRequest = _protector.Encrypt(authorizationRequest);
                    var encodedRequest = _encoder.Encode(encryptedRequest);
                    actionResult.RedirectInstruction.AddParameter(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName,
                        encodedRequest);
                }

                var url = GetRedirectionUrl(this.Request, actionResult.RedirectInstruction.Action);
                var uri = new Uri(url);
                var redirectionUrl = uri.AddParametersInQuery(_actionResultParser.GetRedirectionParameters(actionResult));
                return Redirect(redirectionUrl.ToString());
            }

            return null;
        }
        
        #endregion
        
        #region Private methods
        
        private AuthorizationRequest GetAuthorizationRequestFromJwt(string token,
            string clientId)
        {
            var jwsToken = token;
            if (_jwtParser.IsJweToken(token))
            {
                jwsToken = _jwtParser.Decrypt(token, clientId);
            }

            var jwsPayload = _jwtParser.UnSign(jwsToken, clientId);
            return jwsPayload == null ? null : jwsPayload.ToAuthorizationRequest();
        }
        
        #endregion
        
        #region Private static methods
        
        private static string GetRedirectionUrl(
            Microsoft.AspNet.Http.HttpRequest request,
            IdentityServerEndPoints identityServerEndPoints)
        {
            var uri = request.GetAbsoluteUriWithVirtualPath();
            var partialUri = Constants.MappingIdentityServerEndPointToPartialUrl[identityServerEndPoints];
            return uri + partialUri;
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
                var result = GetAuthorizationRequestFromJwt(authorizationRequest.request,
                    authorizationRequest.client_id);
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
                        var httpClient = new HttpClient
                        {
                            BaseAddress = uri
                        };

                        var httpResult = await httpClient.GetAsync(uri.AbsoluteUri);
                        httpResult.EnsureSuccessStatusCode();
                        var request = await httpResult.Content.ReadAsStringAsync();
                        var result = GetAuthorizationRequestFromJwt(request, authorizationRequest.client_id);
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
        
        #endregion
    }
}