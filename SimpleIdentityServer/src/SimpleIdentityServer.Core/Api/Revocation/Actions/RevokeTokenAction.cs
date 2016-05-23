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
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Core.Api.Revocation.Actions
{
    public interface IRevokeTokenAction
    {

    }

    internal class RevokeTokenAction : IRevokeTokenAction
    {
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;

        private readonly IAuthenticateClient _authenticateClient;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        #region Constructor

        public RevokeTokenAction(
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient,
            IGrantedTokenRepository grantedTokenRepository)
        {
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
            _grantedTokenRepository = grantedTokenRepository;
        }

        #endregion

        #region Public methods

        public void Execute(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (revokeTokenParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter));
            }

            if (string.IsNullOrWhiteSpace(revokeTokenParameter.Token))
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter.Token));
            }

            if (!string.IsNullOrWhiteSpace(revokeTokenParameter.TokenTypeHint) 
                && !Constants.AllStandardTokenTypeHintNames.Contains(revokeTokenParameter.TokenTypeHint))
            {
                throw new IdentityServerException(ErrorCodes.UnsupportedTokenType,
                    string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.RevokeTokenParameterNames.TokenTypeHint));
            }

            var tokenTypeHint = Constants.AllStandardTokenTypeHintNames.Contains(revokeTokenParameter.TokenTypeHint) ?
                revokeTokenParameter.TokenTypeHint : Constants.StandardTokenTypeHintNames.AccessToken;
            // Check the client credentials
            var errorMessage = string.Empty;
            var instruction = CreateAuthenticateInstruction(revokeTokenParameter,
                authenticationHeaderValue);
            var client = _authenticateClient.Authenticate(instruction, out errorMessage);
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    errorMessage);
            }

            // Retrieve the granted token & check if it exists
            GrantedToken grantedToken;
            if (tokenTypeHint == Constants.StandardTokenTypeHintNames.AccessToken)
            {
                grantedToken = _grantedTokenRepository.GetToken(revokeTokenParameter.Token);
            }
            else
            {
                grantedToken = _grantedTokenRepository.GetTokenByRefreshToken(revokeTokenParameter.Token);
            }

            if (grantedToken == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidToken,
                    ErrorDescriptions.TheTokenDoesntExist);
            }

            // Verifies whether the token was issued to the client making the revocation request
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new IdentityServerException(ErrorCodes.InvalidToken,
                    string.Format(ErrorDescriptions.TheTokenHasNotBeenIssuedForTheGivenClientId, client.ClientId));
            }


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
