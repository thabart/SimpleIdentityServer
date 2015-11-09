using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using System.Web.Script.Serialization;
using Microsoft.Practices.ObjectBuilder2;

using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface IJwsGenerator
    {
        JwsPayload GenerateJwsPayload(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter);

        string GenerateJws(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter);
    }

    public class JwsGenerator : IJwsGenerator
    {
        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IClientRepository _clientRepository;

        private readonly IClientValidator _clientValidator;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        private readonly ICreateJwsSignature _createJwsSignature;

        public JwsGenerator(
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IClientRepository clientRepository,
            IClientValidator clientValidator,
            IJsonWebKeyRepository jsonWebKeyRepository,
            ICreateJwsSignature createJwsSignature)
        {
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _createJwsSignature = createJwsSignature;
        }

        public JwsPayload GenerateJwsPayload(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            // Get the issuer from the configuration.
            var issuerName = _simpleIdentityServerConfigurator.GetIssuerName();
            // Audience can be used to disambiguate the intended recipients.
            // Fill-in the list with the list of client_id suspected to parse the JWT tokens.
            var audiences = new List<string>();
            var clients = _clientRepository.GetAll();
            clients.ForEach(client =>
            {
                var isClientSupportIdTokenResponseType =
                    _clientValidator.ValidateResponseType(ResponseType.id_token, client);
                var isClientSupportImplicitGrantTypeFlow =
                    _clientValidator.ValidateGrantType(GrantType.@implicit, client);
                if (isClientSupportIdTokenResponseType && isClientSupportImplicitGrantTypeFlow)
                {
                    audiences.Add(client.ClientId);
                }
            });

            // Calculate the expiration datetime
            var currentDateTime = DateTime.Now;
            var expiredDateTime = currentDateTime.AddSeconds(_simpleIdentityServerConfigurator.GetTokenValidityPeriodInSeconds());
            var expirationInSeconds = expiredDateTime.ConvertToUnixTimestamp();
            // Calculate the time in seconds which the JWT was issued.
            var iatInSeconds = currentDateTime.ConvertToUnixTimestamp();
            // Populate the claims
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, claimPrincipal.GetSubject())
            };

            var result = new JwsPayload
            {
                iss = issuerName,
                aud = audiences.ToArray(),
                exp = expirationInSeconds,
                iat = iatInSeconds,
                Claims = claims
            };

            // If the max_age request is made or when auth_time is requesed as an Essential claim then we calculate the auth_time
            // The auth_time corresponds to the time when the End-User authentication occured. 
            // Its value is a JSON number representing the number of seconds from 1970-01-01
            if (authorizationParameter.MaxAge != 0.0D)
            {
                var authenticationInstant = claimPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
                if (authenticationInstant != null)
                {
                    result.auth_time = double.Parse(authenticationInstant.Value);
                }
            }

            // Set the nonce value in the id token. The value is coming from the authorization request
            if (!string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                result.nonce = authorizationParameter.Nonce;
            }

            // Set the ACR : Authentication Context Class Reference
            // Set the AMR : Authentication Methods Reference
            // For the moment we support a level 1 because only password via HTTPS is supported.
            if (!string.IsNullOrWhiteSpace(authorizationParameter.AcrValues))
            {
                result.acr = Constants.StandardArcParameterNames.OpenIdCustomAuthLevel + ".password=1";
                result.amr = "password";
            }

            // Set the client_id
            // This claim is only needed when the ID token has a single audience value & that audience is different than the authorized party.
            if (audiences != null && 
                audiences.Count() == 1 && 
                audiences.First() == authorizationParameter.ClientId)
            {
                result.azp = authorizationParameter.ClientId;
            }

            // TODO : Add another claims in it ...

            return result;
        }

        public string GenerateJws(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter)
        {
            var client = _clientRepository.GetClientById(authorizationParameter.ClientId);
            var jwsProtectedHeader = GetProtectedHeader(client);

            var javascriptSerializer = new JavaScriptSerializer();
            var jsonJwsProtectedHeader = javascriptSerializer.Serialize(jwsProtectedHeader);
            var jwsProtectedHeaderBase64Encoded = jsonJwsProtectedHeader.Base64Encode();
            var jwsPayLoad = javascriptSerializer.Serialize(jwsPayload);
            var jwsPayLoadBase64Encoded = jwsPayLoad.Base64Encode();

            var combined = string.Format("{0}.{1}", jwsProtectedHeaderBase64Encoded, jwsPayLoadBase64Encoded);

            // Based on the client settings we'll encrypt & signed the id_token in different ways.
            // In the "open-id-connect-discovery" there's an endpoint jwks_uri :
            // This url contains the signing key's) the RP uses to validate signatures from the OP
            // The JWS set may also contain the Server's encryption key(s) which are used by the RP to encrypt requests to the server.
            var signedAlgorithm = (JwsAlg)Enum.Parse(typeof(JwsAlg), jwsProtectedHeader.alg);

            // If there's no signed algorithm then we return the combined result.
            if (signedAlgorithm == JwsAlg.none)
            {
                return combined;
            }

            // If there's no algorithm available to sign the ID_TOKEN then return nothing.
            var algorithms = _jsonWebKeyRepository.GetByAlgorithm(
                Use.Sig, 
                signedAlgorithm.ToAllAlg(),
                new [] { KeyOperations.Sign });
            if (algorithms == null || !algorithms.Any())
            {
                return null;
            }

            var firstAlgorithm = algorithms.First();
            var signedJws = string.Empty;
            switch (firstAlgorithm.Kty)
            {
                case KeyType.RSA:
                    signedJws = _createJwsSignature.SignWithRsa(signedAlgorithm, firstAlgorithm.SerializedKey, combined);
                    break;
            }

            if (signedJws == null)
            {
                throw new IdentityServerExceptionWithState(ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheIdTokenCannotBeSigned,
                    authorizationParameter.State);
            }

            return string.Format("{0}.{1}", combined, signedJws);
        }

        private JwsProtectedHeader GetProtectedHeader(Client client)
        {
            var signedAlgorithm = JwsAlg.RS256;
            if (!string.IsNullOrWhiteSpace(client.IdTokenSignedTResponseAlg))
            {
                signedAlgorithm = client.IdTokenSignedTResponseAlg.ToJwsAlg();
            }

            return new JwsProtectedHeader
            {
                typ = Enum.GetName(typeof(JwsAlg), signedAlgorithm),
                alg = client.IdTokenSignedTResponseAlg
            };
        }
    }
}
