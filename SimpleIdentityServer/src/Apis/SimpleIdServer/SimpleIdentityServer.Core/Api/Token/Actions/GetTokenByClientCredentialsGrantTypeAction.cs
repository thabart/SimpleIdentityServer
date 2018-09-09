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
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.Store;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByClientCredentialsGrantTypeAction
    {
        Task<GrantedToken> Execute(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate = null);
    }

    internal class GetTokenByClientCredentialsGrantTypeAction : IGetTokenByClientCredentialsGrantTypeAction
    {
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IClientValidator _clientValidator;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IScopeValidator _scopeValidator;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;
        private readonly IClientHelper _clientHelper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly ITokenStore _tokenStore;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        #region Constructor

        public GetTokenByClientCredentialsGrantTypeAction(
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient,
            IClientValidator clientValidator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IScopeValidator scopeValidator,
            IOAuthEventSource oauthEventSource,
            IClientCredentialsGrantTypeParameterValidator clientCredentialsGrantTypeParameterValidator,
            IClientHelper clientHelper,
            IJwtGenerator jwtGenerator,
            ITokenStore tokenStore,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
            _clientValidator = clientValidator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = scopeValidator;
            _oauthEventSource = oauthEventSource;
            _clientCredentialsGrantTypeParameterValidator = clientCredentialsGrantTypeParameterValidator;
            _clientHelper = clientHelper;
            _jwtGenerator = jwtGenerator;
            _tokenStore = tokenStore;
            _grantedTokenHelper = grantedTokenHelper;
        }

        #endregion

        #region Public methods

        public async Task<GrantedToken> Execute(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate = null)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);

            // 1. Authenticate the client
            var instruction = CreateAuthenticateInstruction(clientCredentialsGrantTypeParameter, authenticationHeaderValue, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction).ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.client_credentials))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.client_credentials));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseType.token))
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

            // 4. Generate the JWT access token on the fly.
            var grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId).ConfigureAwait(false);
            if (grantedToken == null)
            {
                grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(client, allowedTokenScopes).ConfigureAwait(false);
                await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
                _oauthEventSource.GrantAccessToClient(client.ClientId, grantedToken.AccessToken, allowedTokenScopes);
            }

            return grantedToken;
        }

        #endregion

        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = clientCredentialsGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = clientCredentialsGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = clientCredentialsGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = clientCredentialsGrantTypeParameter.ClientSecret;
            result.Certificate = certificate;
            return result;
        }

        #endregion
    }
}
