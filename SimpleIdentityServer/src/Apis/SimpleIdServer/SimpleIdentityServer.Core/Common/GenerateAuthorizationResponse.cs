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

using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.Store;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Common
{
    public interface IGenerateAuthorizationResponse
    {
        Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Core.Common.Models.Client client);
    }

    public class GenerateAuthorizationResponse : IGenerateAuthorizationResponse
    {
        private readonly IAuthorizationCodeStore _authorizationCodeStore;
        private readonly ITokenStore _tokenStore;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IConsentHelper _consentHelper;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IClientHelper _clientHelper;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GenerateAuthorizationResponse(
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IParameterParserHelper parameterParserHelper,
            IJwtGenerator jwtGenerator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IConsentHelper consentHelper,
            IOAuthEventSource oauthEventSource,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IClientHelper clientHelper,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _authorizationCodeStore = authorizationCodeStore;
            _tokenStore = tokenStore;
            _parameterParserHelper = parameterParserHelper;
            _jwtGenerator = jwtGenerator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _consentHelper = consentHelper;
            _oauthEventSource = oauthEventSource;
            _authorizationFlowHelper = authorizationFlowHelper;
            _clientHelper = clientHelper;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Core.Common.Models.Client client)
        {
            if (actionResult == null || actionResult.RedirectInstruction == null)
            {
                throw new ArgumentNullException(nameof(actionResult));
            }
;
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var newAccessTokenGranted = false;
            var allowedTokenScopes = string.Empty;
            GrantedToken grantedToken = null;
            var newAuthorizationCodeGranted = false;
            AuthorizationCode authorizationCode = null;
            _oauthEventSource.StartGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
                authorizationParameter.ResponseType);
            var responses = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
            var idTokenPayload = await GenerateIdTokenPayload(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            var userInformationPayload = await GenerateUserInformationPayload(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            if (responses.Contains(ResponseType.token)) // 1. Generate an access token.
            {
                if (!string.IsNullOrWhiteSpace(authorizationParameter.Scope))
                {
                    allowedTokenScopes = string.Join(" ", _parameterParserHelper.ParseScopes(authorizationParameter.Scope));
                }

                
                grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId,
                    userInformationPayload, idTokenPayload).ConfigureAwait(false);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(client, allowedTokenScopes,
                        userInformationPayload, idTokenPayload).ConfigureAwait(false);
                    newAccessTokenGranted = true;
                }

                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AccessTokenName,
                    grantedToken.AccessToken);
            }

            if (responses.Contains(ResponseType.code)) // 2. Generate an authorization code.
            {
                var subject = claimsPrincipal == null ? string.Empty : claimsPrincipal.GetSubject();
                var assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(subject, authorizationParameter).ConfigureAwait(false);
                if (assignedConsent != null)
                {
                    // Insert a temporary authorization code 
                    // It will be used later to retrieve tha id_token or an access token.
                    authorizationCode = new AuthorizationCode
                    {
                        Code = Guid.NewGuid().ToString(),
                        RedirectUri = authorizationParameter.RedirectUrl,
                        CreateDateTime = DateTime.UtcNow,
                        ClientId = authorizationParameter.ClientId,
                        Scopes = authorizationParameter.Scope,
                        IdTokenPayload = idTokenPayload,
                        UserInfoPayLoad = userInformationPayload
                    };

                    newAuthorizationCodeGranted = true;
                    actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AuthorizationCodeName,
                        authorizationCode.Code);
                }
            }

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(idTokenPayload,
                authorizationCode == null ? string.Empty : authorizationCode.Code,
                grantedToken == null ? string.Empty : grantedToken.AccessToken,
                authorizationParameter, client);
            
            if (newAccessTokenGranted) // 3. Insert the stateful access token into the DB OR insert the access token into the caching.
            {
                await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
                _oauthEventSource.GrantAccessToClient(authorizationParameter.ClientId,
                    grantedToken.AccessToken,
                    allowedTokenScopes);
            }

            if (newAuthorizationCodeGranted) // 4. Insert the authorization code into the caching.
            {
                if (client.RequirePkce)
                {
                    authorizationCode.CodeChallenge = authorizationParameter.CodeChallenge;
                    authorizationCode.CodeChallengeMethod = authorizationParameter.CodeChallengeMethod;
                }

                await _authorizationCodeStore.AddAuthorizationCode(authorizationCode).ConfigureAwait(false);
                _oauthEventSource.GrantAuthorizationCodeToClient(authorizationParameter.ClientId,
                    authorizationCode.Code,
                    authorizationParameter.Scope);
            }

            if (responses.Contains(ResponseType.id_token))
            {
                var idToken = await GenerateIdToken(idTokenPayload, authorizationParameter).ConfigureAwait(false);
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.IdTokenName, idToken);
            }

            if (!string.IsNullOrWhiteSpace(authorizationParameter.State))
            {
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.StateName, authorizationParameter.State);
            }

            var sessionState = GetSessionState(authorizationParameter.ClientId, authorizationParameter.OriginUrl, authorizationParameter.SessionId);
            if (sessionState != null)
            {
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.SessionState, sessionState);
            }

            if (authorizationParameter.ResponseMode == ResponseMode.form_post)
            {
                actionResult.Type = TypeActionResult.RedirectToAction;
                actionResult.RedirectInstruction.Action = IdentityServerEndPoints.FormIndex;
                actionResult.RedirectInstruction.AddParameter("redirect_uri", authorizationParameter.RedirectUrl);
            }

            // Set the response mode
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(responseTypes,
                        authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                actionResult.RedirectInstruction.ResponseMode = responseMode;
            }

            _oauthEventSource.EndGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
               actionResult.RedirectInstruction.Parameters.SerializeWithJavascript());
        }

        private string GetSessionState(string clientId, string originUrl, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(originUrl))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            var sessionState = string.Empty;
            var salt = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(clientId + originUrl + sessionId + salt);
            byte[] hash;
            using (var sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            var hex = ToHexString(hash);
            return hex.Base64Encode() + "==." + salt;
        }

        public static string ToHexString(IEnumerable<byte> arr)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            var sb = new StringBuilder();
            foreach (var s in arr)
            {
                sb.Append(s.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the JWS payload for identity token.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="jwsPayload"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private async Task<string> GenerateIdToken(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter)
        {
            return await _clientHelper.GenerateIdTokenAsync(authorizationParameter.ClientId,
                jwsPayload).ConfigureAwait(false);
        }

        private async Task<JwsPayload> GenerateIdTokenPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            JwsPayload jwsPayload;
            if (authorizationParameter.Claims != null && 
                authorizationParameter.Claims.IsAnyIdentityTokenClaimParameter())
            {
                jwsPayload = await _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(claimsPrincipal, authorizationParameter, Clone(authorizationParameter.Claims.IdToken)).ConfigureAwait(false);
            }
            else
            {
                jwsPayload = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            }

            return jwsPayload;
        }

        /// <summary>
        /// Generate the JWS payload for user information endpoint.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private async Task<JwsPayload> GenerateUserInformationPayload(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter)
        {
            JwsPayload jwsPayload;
            if (authorizationParameter.Claims != null &&
                authorizationParameter.Claims.IsAnyUserInfoClaimParameter())
            {
                jwsPayload = _jwtGenerator.GenerateFilteredUserInfoPayload(
                    Clone(authorizationParameter.Claims.UserInfo),
                    claimsPrincipal,
                    authorizationParameter);
            }
            else
            {
                jwsPayload = await _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            }

            return jwsPayload;
        }
        
        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return Constants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }

        private static List<ClaimParameter> Clone(List<ClaimParameter> claims)
        {
            var result = new List<ClaimParameter>();
            claims.ForEach(c => result.Add(c));
            return result;
        }
    }
}
