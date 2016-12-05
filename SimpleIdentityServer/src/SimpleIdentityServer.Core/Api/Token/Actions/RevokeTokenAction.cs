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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IRevokeTokenAction
    {
        bool Execute(
               RevokeTokenParameter revokeTokenParameter,
               AuthenticationHeaderValue authenticationHeaderValue);
    }

    internal class RevokeTokenAction : IRevokeTokenAction
    {
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IGrantedTokenRepository _grantedTokenRepository;
        private readonly IClientRepository _clientRepository;

        #region Constructor

        public RevokeTokenAction(
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient,
            IGrantedTokenRepository grantedTokenRepository,
            IClientRepository clientRepository)
        {
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
            _grantedTokenRepository = grantedTokenRepository;
            _clientRepository = clientRepository;
        }

        #endregion

        #region Public methods

        public bool Execute(RevokeTokenParameter revokeTokenParameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (revokeTokenParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter));
            }

            if (string.IsNullOrWhiteSpace(revokeTokenParameter.Token))
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter.Token));
            }
            
            // Check the client credentials
            var errorMessage = string.Empty;
            var instruction = CreateAuthenticateInstruction(revokeTokenParameter,
                authenticationHeaderValue);
            var authResult = _authenticateClient.Authenticate(instruction);
            var client = authResult.Client;
            if (client == null)
            {
                client = _clientRepository.GetClientById(Constants.AnonymousClientId);
                if (client == null)
                {
                    throw new IdentityServerException(ErrorCodes.InternalError,
                        string.Format(ErrorDescriptions.ClientIsNotValid, Constants.AnonymousClientId));
                }
            }

            // Retrieve the granted token & check if it exists
            var isAccessToken = true;
            GrantedToken grantedToken = _grantedTokenRepository.GetToken(revokeTokenParameter.Token);
            if (grantedToken == null)
            {
                isAccessToken = false;
                grantedToken = _grantedTokenRepository.GetTokenByRefreshToken(revokeTokenParameter.Token);
            }

            if (grantedToken == null)
            {
                return false;
            }

            // Verifies whether the token was issued to the client making the revocation request
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new IdentityServerException(ErrorCodes.InvalidToken,
                    string.Format(ErrorDescriptions.TheTokenHasNotBeenIssuedForTheGivenClientId, client.ClientId));
            }

            // Invalid the granted token
            if (!isAccessToken)
            {
                var children = _grantedTokenRepository.GetGrantedTokenChildren(grantedToken.RefreshToken);
                if (children != null && children.Any())
                {
                    foreach (var child in children)
                    {
                        if (!_grantedTokenRepository.Delete(child))
                        {
                            return false;
                        }
                    }
                }

                grantedToken.RefreshToken = string.Empty;
                return _grantedTokenRepository.Update(grantedToken);
            }

            // Revoke the access token & its refresh token.
            return _grantedTokenRepository.Delete(grantedToken);
        }

        #endregion

        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = revokeTokenParameter.ClientAssertion;
            result.ClientAssertionType = revokeTokenParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = revokeTokenParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = revokeTokenParameter.ClientSecret;
            return result;
        }
        
        #endregion
    }
}
