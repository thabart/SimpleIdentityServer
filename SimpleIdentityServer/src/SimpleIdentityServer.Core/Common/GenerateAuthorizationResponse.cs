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

using System;
using System.Security.Claims;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Common
{
    public interface IGenerateAuthorizationResponse
    {
        Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client);
    }

    public class GenerateAuthorizationResponse : IGenerateAuthorizationResponse
    {
        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IGrantedTokenRepository _grantedTokenRepository;
        private readonly IConsentHelper _consentHelper;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IClientHelper _clientHelper;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GenerateAuthorizationResponse(
            IAuthorizationCodeRepository authorizationCodeRepository,
            IParameterParserHelper parameterParserHelper,
            IJwtGenerator jwtGenerator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IGrantedTokenRepository grantedTokenRepository,
            IConsentHelper consentHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IClientHelper clientHelper,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _authorizationCodeRepository = authorizationCodeRepository;
            _parameterParserHelper = parameterParserHelper;
            _jwtGenerator = jwtGenerator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _consentHelper = consentHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authorizationFlowHelper = authorizationFlowHelper;
            _clientHelper = clientHelper;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client)
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
            _simpleIdentityServerEventSource.StartGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
                authorizationParameter.ResponseType);
            var responses = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
            var idTokenPayload = await GenerateIdTokenPayload(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            var userInformationPayload = await GenerateUserInformationPayload(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            if (responses.Contains(ResponseType.token))
            {
                if (!string.IsNullOrWhiteSpace(authorizationParameter.Scope))
                {
                    allowedTokenScopes = string.Join(" ", _parameterParserHelper.ParseScopes(authorizationParameter.Scope));
                }

                // Check if an access token has already been generated and can be reused for that :
                // We assumed that an access token is unique for a specific client id, user information 
                // & id token payload & for certain scopes
                grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(
                    allowedTokenScopes,
                    authorizationParameter.ClientId,
                    idTokenPayload,
                    userInformationPayload).ConfigureAwait(false);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(
                        authorizationParameter.ClientId,
                        allowedTokenScopes,
                        userInformationPayload,
                        idTokenPayload).ConfigureAwait(false);

                    newAccessTokenGranted = true;
                }

                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AccessTokenName,
                    grantedToken.AccessToken);
            }

            if (responses.Contains(ResponseType.code))
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

            if (newAccessTokenGranted)
            {
                await _grantedTokenRepository.InsertAsync(grantedToken).ConfigureAwait(false);
                _simpleIdentityServerEventSource.GrantAccessToClient(authorizationParameter.ClientId,
                    grantedToken.AccessToken,
                    allowedTokenScopes);
            }

            if (newAuthorizationCodeGranted)
            {
                if (client.RequirePkce)
                {
                    authorizationCode.CodeChallenge = authorizationParameter.CodeChallenge;
                    authorizationCode.CodeChallengeMethod = authorizationParameter.CodeChallengeMethod;
                }

                await _authorizationCodeRepository.AddAsync(authorizationCode).ConfigureAwait(false);
                _simpleIdentityServerEventSource.GrantAuthorizationCodeToClient(authorizationParameter.ClientId,
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

            _simpleIdentityServerEventSource.EndGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
               actionResult.RedirectInstruction.Parameters.SerializeWithJavascript());
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
