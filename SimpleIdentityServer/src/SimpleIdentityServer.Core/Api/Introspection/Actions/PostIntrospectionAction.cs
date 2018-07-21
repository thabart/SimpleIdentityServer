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
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Introspection.Actions
{
    public interface IPostIntrospectionAction
    {
        Task<IntrospectionResult> Execute(IntrospectionParameter introspectionParameter, AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class PostIntrospectionAction : IPostIntrospectionAction
    {
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IIntrospectionParameterValidator _introspectionParameterValidator;
        private readonly IGrantedTokenRepository _grantedTokenRepository;

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

        public async Task<IntrospectionResult> Execute(
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
            var instruction = CreateAuthenticateInstruction(introspectionParameter, authenticationHeaderValue);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction).ConfigureAwait(false);
            if (authResult.Client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
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
                grantedToken = await _grantedTokenRepository.GetTokenAsync(introspectionParameter.Token).ConfigureAwait(false);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenRepository.GetTokenByRefreshTokenAsync(introspectionParameter.Token).ConfigureAwait(false);
                }
            }
            else
            {
                grantedToken = await _grantedTokenRepository.GetTokenByRefreshTokenAsync(introspectionParameter.Token).ConfigureAwait(false);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenRepository.GetTokenAsync(introspectionParameter.Token).ConfigureAwait(false);
                }
            }

            // 5. Throw an exception if there's no granted token
            if (grantedToken == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidToken,
                    ErrorDescriptions.TheTokenIsNotValid);
            }

            // 6. Fill-in parameters
            //// TODO : Specifiy the other parameters : NBF & JTI
            var result = new IntrospectionResult
            {
                Scope = grantedToken.Scope,
                ClientId = grantedToken.ClientId,
                Expiration = grantedToken.ExpiresIn,
                TokenType = grantedToken.TokenType
            };

            // 7. Fill-in the other parameters
            if (grantedToken.IdTokenPayLoad != null)
            {
                var audiences = string.Empty;
                var audiencesArr = grantedToken.IdTokenPayLoad.GetArrayClaim(Jwt.Constants.StandardClaimNames.Audiences);
                var issuedAt = grantedToken.IdTokenPayLoad.Iat;
                var issuer = grantedToken.IdTokenPayLoad.Issuer;
                var subject = grantedToken.IdTokenPayLoad.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                var userName = grantedToken.IdTokenPayLoad.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Name);
                if (audiencesArr.Any())
                {
                    audiences = string.Join(" ", audiencesArr);
                }

                result.Audience = audiences;
                result.IssuedAt = issuedAt;
                result.Issuer = issuer;
                result.Subject = subject;
                result.UserName = userName;
            }

            // 8. Based on the expiration date disable OR enable the introspection result
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

        private static string[] GetParameters(string authorizationHeaderValue)
        {
            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }
    }
}
