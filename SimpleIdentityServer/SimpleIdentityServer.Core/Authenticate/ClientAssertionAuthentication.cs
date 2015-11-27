using System;
using System.Linq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
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
        Client AuthenticateClientWithPrivateKeyJwt(
            AuthenticateInstruction instruction,
            out string messageError);

        /// <summary>
        /// Perform client_secret_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="clientSecret"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        Client AuthenticateClientWithClientSecretJwt(
            AuthenticateInstruction instruction,
            string clientSecret,
            out string messageError);
    }

    public class ClientAssertionAuthentication : IClientAssertionAuthentication
    {
        private readonly IJweParser _jweParser;

        private readonly IJwsParser _jwsParser;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IClientValidator _clientValidator;

        public ClientAssertionAuthentication(
            IJweParser jweParser,
            IJwsParser jwsParser,
            IJsonWebKeyRepository jsonWebKeyRepository,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IClientValidator clientValidator)
        {
            _jweParser = jweParser;
            _jwsParser = jwsParser;
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _clientValidator = clientValidator;
        }

        /// <summary>
        /// Perform private_key_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        public Client AuthenticateClientWithPrivateKeyJwt(
            AuthenticateInstruction instruction,
            out string messageError)
        {
            var clientAssertion = instruction.ClientAssertion;
            var clientAssertionSplitted = clientAssertion.Split('.');
            if (clientAssertionSplitted.Count() != 3)
            {
                messageError = ErrorDescriptions.TheClientAssertionIsNotAJwtToken;
                return null;
            }

            var jws = clientAssertion;
            var jwsHeader = _jwsParser.GetHeader(jws);
            var jwsJsonWebKey = _jsonWebKeyRepository.GetByKid(jwsHeader.Kid);
            var payLoad = _jwsParser.ValidateSignature(jws, jwsJsonWebKey);
            if (payLoad == null)
            {
                messageError = ErrorDescriptions.TheSignatureIsNotCorrect;
                return null;
            }

            return ValidateJwsPayLoad(payLoad, out messageError);
        }

        /// <summary>
        /// Perform client_secret_jwt client authentication.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="clientSecret"></param>
        /// <param name="messageError"></param>
        /// <returns></returns>
        public Client AuthenticateClientWithClientSecretJwt(
            AuthenticateInstruction instruction,
            string clientSecret,
            out string messageError)
        {
            var clientAssertion = instruction.ClientAssertion;
            var clientAssertionSplitted = clientAssertion.Split('.');
            if (clientAssertionSplitted.Count() != 5)
            {
                messageError = ErrorDescriptions.TheClientAssertionIsNotAJwtToken;
                return null;
            }

            var jwe = clientAssertion;
            var jweHeader = _jwsParser.GetHeader(jwe);
            var jweJsonWebKey = _jsonWebKeyRepository.GetByKid(jweHeader.Kid);
            var decryptedResult = _jweParser.ParseByUsingSymmetricPassword(jwe, jweJsonWebKey, clientSecret);
            if (string.IsNullOrWhiteSpace(decryptedResult))
            {
                messageError = ErrorDescriptions.TheClientAssertionCannotBeDecrypted;
                return null;
            }

            var decryptedResultSplitted = decryptedResult.Split('.');
            if (decryptedResultSplitted.Count() != 3)
            {
                messageError = ErrorDescriptions.TheClientAssertionCannotBeDecrypted;
                return null;
            }
            
            var jwsHeader = _jwsParser.GetHeader(decryptedResult);
            var jwsJsonWebKey = _jsonWebKeyRepository.GetByKid(jwsHeader.Kid);
            var payLoad = _jwsParser.ValidateSignature(decryptedResult, jwsJsonWebKey);
            if (payLoad == null)
            {
                messageError = ErrorDescriptions.TheSignatureIsNotCorrect;
                return null;
            }

            return ValidateJwsPayLoad(payLoad, out messageError);
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
            var clientAssertionSplitted = clientAssertion.Split('.');
            if (clientAssertionSplitted.Count() != 5
                && clientAssertionSplitted.Count() != 3)
            {
                return string.Empty;
            }
            
            // It's a JWE token then return the client_id from the HTTP body
            if (clientAssertionSplitted.Count() == 5)
            {
                return instruction.ClientIdFromHttpRequestBody;
            }

            // It's a JWS token then return the client_id from the token.
            var jwsHeader = _jwsParser.GetHeader(clientAssertion);
            var jwsJsonWebKey = _jsonWebKeyRepository.GetByKid(jwsHeader.Kid);
            var payLoad = _jwsParser.ValidateSignature(clientAssertion, jwsJsonWebKey);
            if (payLoad == null)
            {
                return string.Empty;
            }

            return payLoad.Issuer;
        }

        private Client ValidateJwsPayLoad(
            JwsPayload jwsPayload,
            out string messageError)
        {
            messageError = string.Empty;
            var expectedIssuer = _simpleIdentityServerConfigurator.GetIssuerName();
            var jwsIssuer = jwsPayload.Issuer;
            var jwsSubject = jwsPayload.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            var jwsAudiences = jwsPayload.Audiences;
            var expirationDateTime = jwsPayload.ExpirationTime.ConvertFromUnixTimestamp();

            Client client = null;
            // 1. Check the issuer is correct.
            if (!string.IsNullOrWhiteSpace(jwsIssuer))
            {
                client = _clientValidator.ValidateClientExist(jwsSubject);
            }

            // 2. Check the client is correct.
            if (client == null || jwsSubject != jwsIssuer)
            {
                messageError = ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect;
                return null;
            }

            // 3. Check if the audience is correct
            if (jwsAudiences == null || !jwsAudiences.Any() || !jwsAudiences.Contains(expectedIssuer))
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
