using System;
using System.Linq;
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
        Client AuthenticateClient(
            AuthenticateInstruction instruction,
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

        public Client AuthenticateClient(
            AuthenticateInstruction instruction,
            out string messageError)
        {
            var clientAssertion = instruction.ClientAssertion;
            var clientAssertionSplitted = clientAssertion.Split('.');
            if (clientAssertionSplitted.Count() != 5 
                && clientAssertionSplitted.Count() != 3)
            {
                messageError = ErrorDescriptions.TheClientAssertionIsNotAJwtToken;
                return null;
            }

            var jws = string.Empty;
            // It's a JWE token.
            if (clientAssertionSplitted.Count() == 5)
            {
                var jweHeader = _jweParser.GetHeader(clientAssertion);
                var jweJsonWebKey = _jsonWebKeyRepository.GetByKid(jweHeader.Kid);
                jws = _jweParser.Parse(clientAssertion, jweJsonWebKey);
            }

            // It's a JWS token
            if (clientAssertionSplitted.Count() == 3)
            {
                jws = clientAssertion;
            }

            if (string.IsNullOrWhiteSpace(jws))
            {
                messageError = ErrorDescriptions.TheJwsPayLoadCannotBeExtractedFromTheClientAssertion;
                return null;
            }

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

            // 1. Check the issuer is correct.
            //if (!string.Equals(expectedIssuer, jwsIssuer))
            //{
            //    messageError = ErrorDescriptions.TheIssuerFromJwtIsNotCorrect;
            //    return null;
            //}

            Client client = null;
            if (!string.IsNullOrWhiteSpace(jwsSubject))
            {
                client = _clientValidator.ValidateClientExist(jwsSubject);
            }

            // 2. Check the client is correct.
            if (client == null)
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
