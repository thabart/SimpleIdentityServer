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
using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Core.Services;
using System.Threading.Tasks;
using System.Linq;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByAuthorizationCodeGrantTypeAction
    {
        Task<GrantedToken> Execute(AuthorizationCodeGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class GetTokenByAuthorizationCodeGrantTypeAction : IGetTokenByAuthorizationCodeGrantTypeAction
    {
        private class ValidationResult
        {
            public AuthorizationCode Code { get; set; }
            public Client Client { get; set; }
        }

        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IClientValidator _clientValidator;
        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IGrantedTokenRepository _grantedTokenRepository;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IClientHelper _clientHelper;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        #region Constructor

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IClientValidator clientValidator,
            IAuthorizationCodeRepository authorizationCodeRepository,
            IConfigurationService configurationService,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IGrantedTokenRepository grantedTokenRepository,
            IAuthenticateClient authenticateClient,
            IClientHelper clientHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _clientValidator = clientValidator;
            _authorizationCodeRepository = authorizationCodeRepository;
            _configurationService = configurationService;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _authenticateClient = authenticateClient;
            _clientHelper = clientHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _grantedTokenHelper = grantedTokenHelper;
        }

        #endregion

        #region Public methods

        public async Task<GrantedToken> Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter, 
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            var result = await ValidateParameter(
                authorizationCodeGrantTypeParameter, 
                authenticationHeaderValue).ConfigureAwait(false);

            // Invalidate the authorization code by removing it !
            await _authorizationCodeRepository.RemoveAsync(result.Code.Code).ConfigureAwait(false);
            var grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(
                result.Code.Scopes,
                result.Code.ClientId,
                result.Code.IdTokenPayload,
                result.Code.UserInfoPayLoad).ConfigureAwait(false);
            if (grantedToken == null)
            {
                grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(
                    result.Code.ClientId,
                    result.Code.Scopes,
                    result.Code.UserInfoPayLoad,
                    result.Code.IdTokenPayload).ConfigureAwait(false);
                await _grantedTokenRepository.InsertAsync(grantedToken).ConfigureAwait(false);
                _simpleIdentityServerEventSource.GrantAccessToClient(
                    result.Code.ClientId,
                    grantedToken.AccessToken,
                    grantedToken.IdToken);
            }

            // Fill-in the id-token
            if (grantedToken.IdTokenPayLoad != null)
            {
                grantedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(result.Client, grantedToken.IdTokenPayLoad).ConfigureAwait(false);
            }

            return grantedToken;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Check the parameters based on the RFC : http://openid.net/specs/openid-connect-core-1_0.html#TokenRequestValidation
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter"></param>
        /// <param name="authenticationHeaderValue"></param>
        /// <returns></returns>
        private async Task<ValidationResult> ValidateParameter(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            // 1. Authenticate the client
            var instruction = CreateAuthenticateInstruction(authorizationCodeGrantTypeParameter, authenticationHeaderValue);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction).ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            var authorizationCode = await _authorizationCodeRepository.GetAsync(authorizationCodeGrantTypeParameter.Code).ConfigureAwait(false);
            // 2. Check if the authorization code is valid
            if (authorizationCode == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
            }

            // 3. Check PKCE
            if (!_clientValidator.CheckPkce(client, authorizationCodeGrantTypeParameter.CodeVerifier, authorizationCode))
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheCodeVerifierIsNotCorrect);
            }

            // 4. Ensure the authorization code was issued to the authenticated client.
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

            // 5. Ensure the authorization code is still valid.
            var authCodeValidity = await _configurationService.GetAuthorizationCodeValidityPeriodInSecondsAsync().ConfigureAwait(false);
            var expirationDateTime = authorizationCode.CreateDateTime.AddSeconds(authCodeValidity);
            var currentDateTime = DateTime.UtcNow;
            if (currentDateTime > expirationDateTime)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsObsolete);
            }

            // Ensure that the redirect_uri parameter value is identical to the redirect_uri parameter value.
            var redirectionUrl = _clientValidator.GetRedirectionUrls(client, authorizationCodeGrantTypeParameter.RedirectUri);
            if (!redirectionUrl.Any())
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, authorizationCodeGrantTypeParameter.RedirectUri));
            }

            return new ValidationResult
            {
                Client = client,
                Code = authorizationCode
            };
        }

        private AuthenticateInstruction CreateAuthenticateInstruction(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = authorizationCodeGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = authorizationCodeGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientSecret;
            return result;
        }

        #endregion
    }
}
