﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Serializers;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Host.Parsers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.Authorization)]
    public class AuthorizationController : Controller
    {
        private readonly IAuthorizationActions _authorizationActions;
        private readonly IDataProtector _dataProtector;
        private readonly IEncoder _encoder;
        private readonly IActionResultParser _actionResultParser;
        private readonly IJwtParser _jwtParser;
        private readonly AuthenticateOptions _authenticateOptions;

        public AuthorizationController(
            IAuthorizationActions authorizationActions,
            IDataProtectionProvider dataProtectionProvider,
            IEncoder encoder,
            IActionResultParser actionResultParser,
            IJwtParser jwtParser,
            AuthenticateOptions authenticateOptions)
        {
            _authorizationActions = authorizationActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _encoder = encoder;
            _actionResultParser = actionResultParser;
            _jwtParser = jwtParser;
            _authenticateOptions = authenticateOptions;
        }

        [HttpGet]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> Get()
        {
            var query = Request.Query;
            if (query == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.RequestIsNotValid);
            }

            var serializer = new ParamSerializer();
            var authorizationRequest = serializer.Deserialize<AuthorizationRequest>(query);
            authorizationRequest = await ResolveAuthorizationRequest(authorizationRequest).ConfigureAwait(false);
            var authenticatedUser = await this.GetAuthenticatedUser(_authenticateOptions.CookieName).ConfigureAwait(false);
            var parameter = authorizationRequest.ToParameter();
            var actionResult = await _authorizationActions.GetAuthorization(parameter, authenticatedUser).ConfigureAwait(false);
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var redirectUrl = new Uri(authorizationRequest.RedirectUri);
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
                        authorizationRequest.Prompt = Enum.GetName(typeof(PromptParameter), PromptParameter.login);
                    }

                    // Set the process id into the request.
                    if (!string.IsNullOrWhiteSpace(actionResult.ProcessId))
                    {
                        authorizationRequest.ProcessId = actionResult.ProcessId;
                    }

                    // Add the encoded request into the query string
                    var encryptedRequest = _dataProtector.Protect(authorizationRequest);
                    actionResult.RedirectInstruction.AddParameter(Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName,
                        encryptedRequest);
                }

                var url = GetRedirectionUrl(this.Request, actionResult.RedirectInstruction.Action);
                var uri = new Uri(url);
                var redirectionUrl = uri.AddParametersInQuery(_actionResultParser.GetRedirectionParameters(actionResult));
                return new RedirectResult(redirectionUrl.AbsoluteUri);
            }

            return null;
        }
        
        private async Task<AuthorizationRequest> GetAuthorizationRequestFromJwt(string token, string clientId)
        {
            var jwsToken = token;
            if (_jwtParser.IsJweToken(token))
            {
                jwsToken = await _jwtParser.DecryptAsync(token, clientId).ConfigureAwait(false);
            }

            var jwsPayload = await _jwtParser.UnSignAsync(jwsToken, clientId).ConfigureAwait(false);
            return jwsPayload == null ? null : jwsPayload.ToAuthorizationRequest();
        }
        
        private static string GetRedirectionUrl(
            Microsoft.AspNetCore.Http.HttpRequest request,
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
            if (!string.IsNullOrWhiteSpace(authorizationRequest.Request))
            {
                var result = await GetAuthorizationRequestFromJwt(authorizationRequest.Request, authorizationRequest.ClientId).ConfigureAwait(false);
                if (result == null)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidRequestCode, ErrorDescriptions.TheRequestParameterIsNotCorrect, authorizationRequest.State);
                }

                return result;
            }

            if (!string.IsNullOrWhiteSpace(authorizationRequest.RequestUri))
            {
                Uri uri;
                if (Uri.TryCreate(authorizationRequest.RequestUri, UriKind.Absolute, out uri))
                {
                    try
                    {
                        var httpClient = new HttpClient
                        {
                            BaseAddress = uri
                        };

                        var httpResult = await httpClient.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);
                        httpResult.EnsureSuccessStatusCode();
                        var request = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var result = await GetAuthorizationRequestFromJwt(request, authorizationRequest.ClientId).ConfigureAwait(false);
                        if (result == null)
                        {
                            throw new IdentityServerExceptionWithState(
                                ErrorCodes.InvalidRequestCode,
                                ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                                authorizationRequest.State);
                        }

                        return result;
                    }
                    catch (Exception)
                    {
                        throw new IdentityServerExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                            authorizationRequest.State);
                    }
                }

                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRequestUriParameterIsNotWellFormed,
                    authorizationRequest.State);
            }

            return authorizationRequest;
        }
    }
}