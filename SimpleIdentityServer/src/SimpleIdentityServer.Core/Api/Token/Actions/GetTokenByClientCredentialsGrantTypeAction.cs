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

using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByClientCredentialsGrantTypeAction
    {
        Task<GrantedToken> Execute(
               ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
               AuthenticationHeaderValue authenticationHeaderValue);
    }

    internal class GetTokenByClientCredentialsGrantTypeAction : IGetTokenByClientCredentialsGrantTypeAction
    {
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IClientValidator _clientValidator;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IScopeValidator _scopeValidator;
        private readonly IGrantedTokenRepository _grantedTokenRepository;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;
        private readonly IClientHelper _clientHelper;
        private readonly IGrantedTokenHelper _grantedTokenHelper;
        private readonly IJwtGenerator _jwtGenerator;

        #region Constructor

        public GetTokenByClientCredentialsGrantTypeAction(
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient,
            IClientValidator clientValidator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IScopeValidator scopeValidator,
            IGrantedTokenRepository grantedTokenRepository,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IClientCredentialsGrantTypeParameterValidator clientCredentialsGrantTypeParameterValidator,
            IClientHelper clientHelper,
            IGrantedTokenHelper grantedTokenHelper,
            IJwtGenerator jwtGenerator)
        {
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
            _clientValidator = clientValidator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = scopeValidator;
            _grantedTokenRepository = grantedTokenRepository;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _clientCredentialsGrantTypeParameterValidator = clientCredentialsGrantTypeParameterValidator;
            _clientHelper = clientHelper;
            _grantedTokenHelper = grantedTokenHelper;
            _jwtGenerator = jwtGenerator;
        }

        #endregion

        #region Public methods

        public async Task<GrantedToken> Execute(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);

            // 1. Authenticate the client
            var instruction = CreateAuthenticateInstruction(clientCredentialsGrantTypeParameter,
                authenticationHeaderValue);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction);
            var client = authResult.Client;
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null ||
                !client.GrantTypes.Contains(GrantType.client_credentials))
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.client_credentials));
            }

            if (client.ResponseTypes == null ||
                !client.ResponseTypes.Contains(ResponseType.token))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, client.ClientId, ResponseType.token));
            }

            // 3. Check scopes
            string allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                var scopeValidation = _scopeValidator.Check(clientCredentialsGrantTypeParameter.Scope, client);
                if (!scopeValidation.IsValid)
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidScope,
                        scopeValidation.ErrorMessage);
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            GrantedToken grantedToken = null;
            // 4. If the access token is stateless then returns a JWT token.
            if (client.AccessTokenState == AccessTokenStates.Stateless)
            {
                var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, allowedTokenScopes.Split(' '));
                var idToken = await _clientHelper.GenerateIdTokenAsync(client, jwsPayload);
                grantedToken = await _grantedTokenGeneratorHelper.GenerateToken(client.ClientId, idToken, allowedTokenScopes);
            }
            else
            {
                // 5. If the access token is stateful then generate a new one and persist it into the DB.
                grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(client.ClientId, allowedTokenScopes);
                    await _grantedTokenRepository.InsertAsync(grantedToken);
                }
            }

            _simpleIdentityServerEventSource.GrantAccessToClient(client.ClientId, grantedToken.AccessToken, allowedTokenScopes);
            return grantedToken;
        }

        #endregion

        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = clientCredentialsGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = clientCredentialsGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = clientCredentialsGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = clientCredentialsGrantTypeParameter.ClientSecret;
            return result;
        }

        #endregion
    }
}
