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
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Core.Api.Introspection.Actions
{
    public interface IPostIntrospectionAction
    {
        IntrospectionResult Execute(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class PostIntrospectionAction : IPostIntrospectionAction
    {
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        private readonly IAuthenticateClient _authenticateClient;

        private readonly IIntrospectionParameterValidator _introspectionParameterValidator;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        #region Constructor

        public PostIntrospectionAction(
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthenticateClient authenticateClient,
            IIntrospectionParameterValidator introspectionParameterValidator,
            IGrantedTokenRepository grantedTokenRepository)
        {
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authenticateClient = authenticateClient;
            _introspectionParameterValidator = introspectionParameterValidator;
            _grantedTokenRepository = grantedTokenRepository;
        }

        #endregion

        #region Public methods

        public IntrospectionResult Execute(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {            
            // 1. Validate the parameters
            if (introspectionParameter == null)
            {
                throw new ArgumentNullException(nameof(introspectionParameter));
            }

            _introspectionParameterValidator.Validate(introspectionParameter);

            // 2. Authenticate the client
            var errorMessage = string.Empty;
            var instruction = CreateAuthenticateInstruction(introspectionParameter,
                authenticationHeaderValue);
            var client = _authenticateClient.Authenticate(instruction, out errorMessage);
            if (client == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClient,
                    errorMessage);
            }

            // 3. Retrieve the token type hint
            var tokenTypeHint = Constants.StandardTokenTypeHintNames.AccessToken;
            if (Constants.AllStandardTokenTypeHintNames.Contains(introspectionParameter.TokenTypeHint))
            {
                tokenTypeHint = introspectionParameter.TokenTypeHint;
            }

            // 4. Trying to fetch the information about the access_token  || refresh_token
            GrantedToken grantedToken = null;
            if (tokenTypeHint == Constants.StandardTokenTypeHintNames.AccessToken)
            {
                grantedToken = _grantedTokenRepository.GetToken(introspectionParameter.Token);
                if (grantedToken == null)
                {
                    grantedToken = _grantedTokenRepository.GetTokenByRefreshToken(introspectionParameter.Token);
                }
            }
            else
            {
                grantedToken = _grantedTokenRepository.GetTokenByRefreshToken(introspectionParameter.Token);
                if (grantedToken == null)
                {
                    grantedToken = _grantedTokenRepository.GetToken(introspectionParameter.Token);
                }
            }

            // 5. Throw an exception if there's no granted token
            if (grantedToken == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidToken,
                    ErrorDescriptions.TheTokenIsNotValid);
            }

            // 6. Throw an exception if the token has not been issued for the client
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidToken,
                    string.Format(ErrorDescriptions.TheTokenHasNotBeenIssuedForTheGivenClientId, client.ClientId));
            }

            // 8. Fill-in the other parameter
            var audiences = string.Empty;
            var audiencesArr = grantedToken.UserInfoPayLoad.GetArrayClaim(Jwt.Constants.StandardClaimNames.Audiences);
            var issuedAt = grantedToken.UserInfoPayLoad.Iat;
            var issuer = grantedToken.UserInfoPayLoad.Issuer;
            var subject = grantedToken.UserInfoPayLoad.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            var userName = grantedToken.UserInfoPayLoad.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Name);
            if (audiencesArr.Any())
            {
                audiences = string.Join(" ", audiencesArr);
            }

            /// TODO : Specifiy the other parameters : NBF & JTI
            var result = new IntrospectionResult
            {
                Scope = grantedToken.Scope,
                ClientId = grantedToken.ClientId,
                Expiration = grantedToken.ExpiresIn,
                TokenType = grantedToken.TokenType,
                Audience = audiences,
                IssuedAt = issuedAt,
                Issuer = issuer,
                Subject = subject,
                UserName = userName
            };

            // 7. Based on the expiration date disable OR enable the introspection result
            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTime.UtcNow > expirationDateTime;
            if (tokenIsExpired)
            {
                result.Active = false;
            }
            else
            {
                result.Active = true;
            }

            return result;
        }

        #endregion

        #region Private methods

        private AuthenticateInstruction CreateAuthenticateInstruction(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = new AuthenticateInstruction
            {
                ClientAssertion = introspectionParameter.ClientAssertion,
                ClientAssertionType = introspectionParameter.ClientAssertionType,
                ClientIdFromHttpRequestBody = introspectionParameter.ClientId,
                ClientSecretFromHttpRequestBody = introspectionParameter.ClientSecret
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

        #endregion

        #region Private static methods

        private static string[] GetParameters(string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                return new string[0];
            }

            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }

        #endregion
    }
}
