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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByClientCredentialsGrantTypeAction
    {
        GrantedToken Execute(
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
            IClientHelper clientHelper)
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
        }

        #endregion

        #region Public methods

        public GrantedToken Execute(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);

            // Authenticate the client
            var errorMessage = string.Empty;
            var instruction = CreateAuthenticateInstruction(clientCredentialsGrantTypeParameter,
                authenticationHeaderValue);
            var client = _authenticateClient.Authenticate(instruction, out errorMessage);
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    errorMessage);
            }

            // Check client
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

            // Check scopes
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                string messageErrorDescription;
                var scopes = _scopeValidator.IsScopesValid(clientCredentialsGrantTypeParameter.Scope, client, out messageErrorDescription);
                if (scopes == null ||
                    !scopes.Any())
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidScope,
                        messageErrorDescription);
                }

                allowedTokenScopes = string.Join(" ", scopes);
            }

            // Generate token
            var generatedToken = _grantedTokenGeneratorHelper.GenerateToken(
                client.ClientId,
                allowedTokenScopes);
            _grantedTokenRepository.Insert(generatedToken);

            _simpleIdentityServerEventSource.GrantAccessToClient(client.ClientId,
                generatedToken.AccessToken,
                allowedTokenScopes);

            return generatedToken;
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
