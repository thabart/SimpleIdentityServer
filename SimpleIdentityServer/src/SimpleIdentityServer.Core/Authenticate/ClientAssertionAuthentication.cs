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
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientAssertionAuthentication
    {
        string GetClientId(AuthenticateInstruction instruction);

        /// <summary>
        /// Perform private_key_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        Models.Client AuthenticateClientWithPrivateKeyJwt(
            AuthenticateInstruction instruction,
            out string messageError);

        /// <summary>
        /// Perform client_secret_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="clientSecret"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        Models.Client AuthenticateClientWithClientSecretJwt(
            AuthenticateInstruction instruction,
            string clientSecret,
            out string messageError);
    }

    public class ClientAssertionAuthentication : IClientAssertionAuthentication
    {
        private readonly IJwsParser _jwsParser;
        
        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IClientValidator _clientValidator;

        private readonly IJwtParser _jwtParser;

        public ClientAssertionAuthentication(
            IJwsParser jwsParser,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IClientValidator clientValidator,
            IJwtParser jwtParser)
        {
            _jwsParser = jwsParser;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _clientValidator = clientValidator;
            _jwtParser = jwtParser;
        }

        /// <summary>
        /// Perform private_key_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        public Models.Client AuthenticateClientWithPrivateKeyJwt(
            AuthenticateInstruction instruction,
            out string messageError)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }
            
            var clientAssertion = instruction.ClientAssertion;
            var isJwsToken = _jwtParser.IsJwsToken(clientAssertion);
            if (!isJwsToken)
            {
                messageError = ErrorDescriptions.TheClientAssertionIsNotAJwsToken;
                return null;
            }

            var jws = instruction.ClientAssertion;
            var jwsPayload = _jwsParser.GetPayload(jws);
            if (jwsPayload == null)
            {
                messageError = ErrorDescriptions.TheJwsPayloadCannotBeExtracted;
                return null;
            }

            var clientId = jwsPayload.Issuer;
            var payload = _jwtParser.UnSign(jws,
                clientId);
            if (payload == null)
            {
                messageError = ErrorDescriptions.TheSignatureIsNotCorrect;
                return null;
            }

            return ValidateJwsPayLoad(payload, out messageError);
        }

        /// <summary>
        /// Perform client_secret_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="clientSecret"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        public Models.Client AuthenticateClientWithClientSecretJwt(
            AuthenticateInstruction instruction,
            string clientSecret,
            out string messageError)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }

            var clientAssertion = instruction.ClientAssertion;
            var isJweToken = _jwtParser.IsJweToken(clientAssertion);
            if (!isJweToken)
            {
                messageError = ErrorDescriptions.TheClientAssertionIsNotAJweToken;
                return null;
            }

            var jwe = instruction.ClientAssertion;
            var clientId = instruction.ClientIdFromHttpRequestBody;
            var jws = _jwtParser.DecryptWithPassword(jwe,
                clientId,
                clientSecret);
            if (string.IsNullOrWhiteSpace(jws))
            {
                messageError = ErrorDescriptions.TheJweTokenCannotBeDecrypted;
                return null;
            }

            var isJwsToken = _jwtParser.IsJwsToken(jws);
            if (!isJwsToken)
            {
                messageError = ErrorDescriptions.TheClientAssertionIsNotAJwsToken;
                return null;
            }

            var jwsPayload = _jwtParser.UnSign(jws,
                clientId);
            if (jwsPayload == null)
            {
                messageError = ErrorDescriptions.TheJwsPayloadCannotBeExtracted;
                return null;
            }

            return ValidateJwsPayLoad(jwsPayload, out messageError);
        }

        /// <summary>
        /// Try to get the client id.
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public string GetClientId(AuthenticateInstruction instruction)
        {
            if (instruction.ClientAssertionType != Constants.StandardClientAssertionTypes.JwtBearer || string.IsNullOrWhiteSpace(instruction.ClientAssertion))
            {
                return string.Empty;
            }

            var clientAssertion = instruction.ClientAssertion;
            var isJweToken = _jwtParser.IsJweToken(clientAssertion);
            var isJwsToken = _jwtParser.IsJwsToken(clientAssertion);
            if (isJweToken && isJwsToken)
            {
                return string.Empty;
            }
            
            // It's a JWE token then return the client_id from the HTTP body
            if (isJweToken)
            {
                return instruction.ClientIdFromHttpRequestBody;
            }

            // It's a JWS token then return the client_id from the token.
            var payload = _jwsParser.GetPayload(clientAssertion);
            if (payload == null)
            {
                return string.Empty;
            }

            return payload.Issuer;
        }

        private Models.Client ValidateJwsPayLoad(
            JwsPayload jwsPayload,
            out string messageError)
        {
            // The checks are coming from this url : http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
            messageError = string.Empty;
            var expectedIssuer = _simpleIdentityServerConfigurator.GetIssuerName();
            var jwsIssuer = jwsPayload.Issuer;
            var jwsSubject = jwsPayload.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            var jwsAudiences = jwsPayload.Audiences;
            var expirationDateTime = jwsPayload.ExpirationTime.ConvertFromUnixTimestamp();

            Models.Client client = null;
            // 1. Check the issuer is correct.
            if (!string.IsNullOrWhiteSpace(jwsIssuer))
            {
                client = _clientValidator.ValidateClientExist(jwsIssuer);
            }

            // 2. Check the client is correct.
            if (client == null || jwsSubject != jwsIssuer)
            {
                messageError = ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect;
                return null;
            }

            // 3. Check if the audience is correct
            if (jwsAudiences == null || 
                !jwsAudiences.Any() || 
                !jwsAudiences.Any(j => j.Contains(expectedIssuer)))
            {
                messageError = ErrorDescriptions.TheAudiencePassedInJwtIsNotCorrect;
                return null;
            }

            // 4. Check the expiration time
            if (DateTime.UtcNow > expirationDateTime)
            {
                messageError = ErrorDescriptions.TheReceivedJwtHasExpired;
                return null;
            }

            return client;
        }
    }
}
