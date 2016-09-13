#region copyright
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

namespace SimpleIdentityServer.Core.Common
{
    public interface IGenerateAuthorizationResponse
    {
        void Execute(
            ActionResult actionResult,
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal);
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

        #region Public methods

        public void Execute(
            ActionResult actionResult, 
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            if (actionResult == null || actionResult.RedirectInstruction == null)
            {
                throw new ArgumentNullException("actionResult");
            }


            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");    
            }

            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException("claimsPrincipal");
            }

            var newAccessTokenGranted = false;
            var allowedTokenScopes = new List<string>();
            GrantedToken grantedToken = null;
            var newAuthorizationCodeGranted = false;
            AuthorizationCode authorizationCode = null;

            _simpleIdentityServerEventSource.StartGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
                authorizationParameter.ResponseType);

            var responses = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
            var idTokenPayload = GenerateIdTokenPayload(claimsPrincipal, authorizationParameter);
            var userInformationPayload = GenerateUserInformationPayload(claimsPrincipal, authorizationParameter);

            if (responses.Contains(ResponseType.token))
            {
                if (!string.IsNullOrWhiteSpace(authorizationParameter.Scope))
                {
                    allowedTokenScopes = _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
                }

                // Check if an access token has already been generated and can be reused for that :
                // We assumed that an access token is unique for a specific client id, user information 
                // & id token payload & for certain scopes
                grantedToken = _grantedTokenHelper.GetValidGrantedToken(
                    allowedTokenScopes.Concat(),
                    authorizationParameter.ClientId,
                    idTokenPayload,
                    userInformationPayload);
                if (grantedToken == null)
                {
                    grantedToken = _grantedTokenGeneratorHelper.GenerateToken(
                        authorizationParameter.ClientId,
                        allowedTokenScopes,
                        userInformationPayload,
                        idTokenPayload);

                    newAccessTokenGranted = true;
                }
                
                actionResult.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AccessTokenName, 
                    grantedToken.AccessToken);
            }

            if (responses.Contains(ResponseType.code))
            {
                var subject = claimsPrincipal == null ? string.Empty : claimsPrincipal.GetSubject();
                var assignedConsent = _consentHelper.GetConsentConfirmedByResourceOwner(subject, authorizationParameter);
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
                authorizationParameter);

            if (newAccessTokenGranted)
            {
                _grantedTokenRepository.Insert(grantedToken);
                _simpleIdentityServerEventSource.GrantAccessToClient(authorizationParameter.ClientId,
                    grantedToken.AccessToken,
                    allowedTokenScopes.Concat());
            }

            if (newAuthorizationCodeGranted)
            {
                _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);
                _simpleIdentityServerEventSource.GrantAuthorizationCodeToClient(authorizationParameter.ClientId,
                    authorizationCode.Code,
                    authorizationParameter.Scope);
            }

            if (responses.Contains(ResponseType.id_token))
            {
                var idToken = GenerateIdToken(idTokenPayload, authorizationParameter);
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
                    var responseTypes = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
                    var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(responseTypes,
                        authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                actionResult.RedirectInstruction.ResponseMode = responseMode;
            }

            _simpleIdentityServerEventSource.EndGeneratingAuthorizationResponseToClient(authorizationParameter.ClientId,
               actionResult.RedirectInstruction.Parameters.SerializeWithJavascript());
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Generate the JWS payload for identity token.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="jwsPayload"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private string GenerateIdToken(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter)
        {
            return _clientHelper.GenerateIdToken(authorizationParameter.ClientId,
                jwsPayload);
        }

        private JwsPayload GenerateIdTokenPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            JwsPayload jwsPayload;
            if (authorizationParameter.Claims != null &&
                authorizationParameter.Claims.IsAnyIdentityTokenClaimParameter())
            {
                jwsPayload = _jwtGenerator.GenerateFilteredIdTokenPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    Clone(authorizationParameter.Claims.IdToken));
            }
            else
            {
                jwsPayload = _jwtGenerator.GenerateIdTokenPayloadForScopes(claimsPrincipal, authorizationParameter);
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
        private JwsPayload GenerateUserInformationPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
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
                jwsPayload = _jwtGenerator.GenerateUserInfoPayloadForScope(claimsPrincipal, authorizationParameter);
            }

            return jwsPayload;
        }

        #endregion

        #region Private static methods
        
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

        #endregion
    }
}
