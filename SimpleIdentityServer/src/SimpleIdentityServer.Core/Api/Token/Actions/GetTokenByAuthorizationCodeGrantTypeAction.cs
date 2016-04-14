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
using System.Linq;
using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByAuthorizationCodeGrantTypeAction
    {
        GrantedToken Execute(AuthorizationCodeGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class GetTokenByAuthorizationCodeGrantTypeAction : IGetTokenByAuthorizationCodeGrantTypeAction
    {
        private readonly IClientValidator _clientValidator;

        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IAuthenticateClient _authenticateClient;

        private readonly IClientHelper _clientHelper;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IClientValidator clientValidator,
            IAuthorizationCodeRepository authorizationCodeRepository,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IGrantedTokenRepository grantedTokenRepository,
            IAuthenticateClient authenticateClient,
            IClientHelper clientHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _clientValidator = clientValidator;
            _authorizationCodeRepository = authorizationCodeRepository;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _authenticateClient = authenticateClient;
            _clientHelper = clientHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public GrantedToken Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter, 
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            var authorizationCode = ValidateParameter(
                authorizationCodeGrantTypeParameter, 
                authenticationHeaderValue);

            // Invalide the authorization code by removing it !
            _authorizationCodeRepository.RemoveAuthorizationCode(authorizationCode.Code);
            var grantedToken = _grantedTokenRepository.GetToken(
                authorizationCode.Scopes,
                authorizationCode.ClientId,
                authorizationCode.IdTokenPayload,
                authorizationCode.UserInfoPayLoad);
            if (grantedToken == null)
            {
                grantedToken = _grantedTokenGeneratorHelper.GenerateToken(
                    authorizationCode.ClientId,
                    authorizationCode.Scopes,
                    authorizationCode.UserInfoPayLoad,
                    authorizationCode.IdTokenPayload);
                _grantedTokenRepository.Insert(grantedToken);
                _simpleIdentityServerEventSource.GrantAccessToClient(
                    authorizationCode.ClientId,
                    grantedToken.AccessToken,
                    grantedToken.IdToken);
            }

            if (grantedToken.IdTokenPayLoad != null)
            {
                grantedToken.IdToken = _clientHelper.GenerateIdToken(
                    grantedToken.ClientId,
                    grantedToken.IdTokenPayLoad);
            }

            return grantedToken;
        }

        /// <summary>
        /// Check the parameters based on the RFC : http://openid.net/specs/openid-connect-core-1_0.html#TokenRequestValidation
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter"></param>
        /// <param name="authenticationHeaderValue"></param>
        /// <returns></returns>
        private AuthorizationCode ValidateParameter(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            // Authenticate the client
            var errorMessage = string.Empty;
            var instruction = CreateAuthenticateInstruction(authorizationCodeGrantTypeParameter,
                authenticationHeaderValue);
            var client = _authenticateClient.Authenticate(instruction, out errorMessage);
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    errorMessage);
            }

            var authorizationCode = _authorizationCodeRepository.GetAuthorizationCode(authorizationCodeGrantTypeParameter.Code);
            // Check if the authorization code is valid
            if (authorizationCode == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
            }

            // Ensure the authorization code was issued to the authenticated client.
            var authorizationClientId = authorizationCode.ClientId;
            if (authorizationClientId != client.ClientId)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                        client.ClientId));
            }

            if (authorizationCode.RedirectUri != authorizationCodeGrantTypeParameter.RedirectUri)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRedirectionUrlIsNotTheSame);
            }

            // Ensure the authorization code is still valid.
            var authCodeValidity = _simpleIdentityServerConfigurator.GetAuthorizationCodeValidityPeriodInSeconds();
            var expirationDateTime = authorizationCode.CreateDateTime.AddSeconds(authCodeValidity);
            var currentDateTime = DateTime.UtcNow;
            if (currentDateTime > expirationDateTime)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsObsolete);
            }

            // Ensure that the redirect_uri parameter value is identical to the redirect_uri parameter value.
            var redirectionUrl = _clientValidator.ValidateRedirectionUrl(authorizationCodeGrantTypeParameter.RedirectUri, client);
            if (redirectionUrl == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, authorizationCodeGrantTypeParameter.RedirectUri));
            }

            return authorizationCode;
        }

        private AuthenticateInstruction CreateAuthenticateInstruction(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = new AuthenticateInstruction
            {
                ClientAssertion = authorizationCodeGrantTypeParameter.ClientAssertion,
                ClientAssertionType = authorizationCodeGrantTypeParameter.ClientAssertionType,
                ClientIdFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientId,
                ClientSecretFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientSecret
            };

            if (authenticationHeaderValue != null 
                && !string.IsNullOrWhiteSpace(authenticationHeaderValue.Parameter))
            {
                var parameters = GetParameters(authenticationHeaderValue.Parameter);
                if (parameters != null && parameters.Count() == 2)
                {
                    result.ClientIdFromAuthorizationHeader = parameters[0];
                    result.ClientSecretFromAuthorizationHeader = parameters[1];
                }
            }

            return result;
        }
        
        private static string[] GetParameters(string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                return new string[0];
            }

            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }
    }
}
