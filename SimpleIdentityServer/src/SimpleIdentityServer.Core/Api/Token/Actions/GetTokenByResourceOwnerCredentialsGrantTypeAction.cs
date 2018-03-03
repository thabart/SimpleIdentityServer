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
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Stores;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        Task<GrantedToken> Execute(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate = null);
    }

    public class GetTokenByResourceOwnerCredentialsGrantTypeAction : IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IScopeValidator _scopeValidator;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IClientRepository _clientRepository;
        private readonly IClientHelper _clientHelper;
        private readonly ITokenStore _tokenStore;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IScopeValidator scopeValidator,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthenticateClient authenticateClient,
            IJwtGenerator jwtGenerator,
            IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IClientRepository clientRepository,
            IClientHelper clientHelper,
            ITokenStore tokenStore,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = scopeValidator;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _clientRepository = clientRepository;
            _clientHelper = clientHelper;
            _tokenStore = tokenStore;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task<GrantedToken> Execute(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate = null)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            // 1. Try to authenticate the client
            var instruction = CreateAuthenticateInstruction(resourceOwnerGrantTypeParameter, authenticationHeaderValue, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction);
            var client = authResult.Client;
            if (authResult.Client == null)
            {
                _simpleIdentityServerEventSource.Info(ErrorDescriptions.TheClientCannotBeAuthenticated);
                client = await _clientRepository.GetClientByIdAsync(Constants.AnonymousClientId);
                if (client == null)
                {
                    throw new IdentityServerException(ErrorCodes.InternalError, string.Format(ErrorDescriptions.ClientIsNotValid, Constants.AnonymousClientId));
                }
            }

            // 2. Try to authenticate a resource owner
            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(resourceOwnerGrantTypeParameter.UserName, resourceOwnerGrantTypeParameter.Password);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant, ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            // 3. Check if the requested scopes are valid
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                var scopeValidation = _scopeValidator.Check(resourceOwnerGrantTypeParameter.Scope, client);
                if (!scopeValidation.IsValid)
                {
                    throw new IdentityServerException(ErrorCodes.InvalidScope, scopeValidation.ErrorMessage);
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            // 4. Generate the user information payload and store it.
            var claims = resourceOwner.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, "simpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = resourceOwnerGrantTypeParameter.Scope
            };
            var payload = await _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(claimsPrincipal, authorizationParameter);
            var generatedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId, payload, payload);
            if (generatedToken == null)
            {
                generatedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(client, allowedTokenScopes, payload, payload);
                await _tokenStore.AddToken(generatedToken);
                _simpleIdentityServerEventSource.GrantAccessToClient(client.ClientId, generatedToken.AccessToken, allowedTokenScopes);
            }

            if (generatedToken.IdTokenPayLoad != null)
            {
                generatedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(client, generatedToken.IdTokenPayLoad);
            }

            return generatedToken;
        }
        
        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = resourceOwnerGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = resourceOwnerGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = resourceOwnerGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = resourceOwnerGrantTypeParameter.ClientSecret;
            result.Certificate = certificate;
            return result;
        }

        #endregion
    }
}