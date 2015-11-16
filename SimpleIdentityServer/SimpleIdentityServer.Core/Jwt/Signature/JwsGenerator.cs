using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using Microsoft.Practices.ObjectBuilder2;

using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Mapping;
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
        private const string JwsType = "JWT";

        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IClientRepository _clientRepository;

        private readonly IClientValidator _clientValidator;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        private readonly ICreateJwsSignature _createJwsSignature;

        private readonly IScopeRepository _scopeRepository;

        private readonly IClaimsMapping _claimsMapping;

        private readonly IParameterParserHelper _parameterParserHelper;

        public JwsGenerator(
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IClientRepository clientRepository,
            IClientValidator clientValidator,
            IJsonWebKeyRepository jsonWebKeyRepository,
            ICreateJwsSignature createJwsSignature,
            IScopeRepository scopeRepository,
            IClaimsMapping claimsMapping,
            IParameterParserHelper parameterParserHelper)
        {
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _createJwsSignature = createJwsSignature;
            _scopeRepository = scopeRepository;
            _claimsMapping = claimsMapping;
            _parameterParserHelper = parameterParserHelper;
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
                if (isClientSupportIdTokenResponseType)
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
            var scopes = _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
            var claims = GetClaimsFromRequestedScopes(scopes, claimPrincipal);

            var result = new JwsPayload
            {
                {
                    Constants.StandardClaimNames.Issuer, issuerName
                },
                {
                    Constants.StandardClaimNames.Audiences, audiences.ToArray()
                },
                {
                    Constants.StandardClaimNames.ExpirationTime, expirationInSeconds
                },
                {
                    Constants.StandardClaimNames.Iat, iatInSeconds
                }
            };

            foreach (var claim in claims)
            {
                result.Add(claim.Key, claim.Value);
            }

            // If the max_age request is made or when auth_time is requesed as an Essential claim then we calculate the auth_time
            // The auth_time corresponds to the time when the End-User authentication occured. 
            // Its value is a JSON number representing the number of seconds from 1970-01-01
            if (authorizationParameter.MaxAge != 0.0D)
            {
                var authenticationInstant = claimPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
                if (authenticationInstant != null)
                {
                    result.Add(Constants.StandardClaimNames.AuthenticationTime, double.Parse(authenticationInstant.Value));
                }
            }

            // Set the nonce value in the id token. The value is coming from the authorization request
            if (!string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                result.Add(Constants.StandardClaimNames.Nonce, authorizationParameter.Nonce);
            }

            // Set the ACR : Authentication Context Class Reference
            // Set the AMR : Authentication Methods Reference
            // For the moment we support a level 1 because only password via HTTPS is supported.
            /*if (!string.IsNullOrWhiteSpace(authorizationParameter.AcrValues))
            {*/
            result.Add(Constants.StandardClaimNames.Acr, Constants.StandardArcParameterNames.OpenIdCustomAuthLevel + ".password=1");
            result.Add(Constants.StandardClaimNames.Amr, "password");
            //}

            // Set the client_id
            // This claim is only needed when the ID token has a single audience value & that audience is different than the authorized party.
            if (audiences != null && 
                audiences.Count() == 1 && 
                audiences.First() == authorizationParameter.ClientId)
            {
                result.Add(Constants.StandardClaimNames.Azp, authorizationParameter.ClientId);
            }

            // TODO : Add another claims in it ...

            return result;
        }

        public string GenerateJws(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter)
        {
            var client = _clientRepository.GetClientById(authorizationParameter.ClientId);
            var jwsProtectedHeader = ConstructProtectedHeader(client);

            // In the "open-id-connect-discovery" there's an endpoint jwks_uri :
            // This url contains the signing key's) the RP uses to validate signatures from the OP
            // The JWS set may also contain the Server's encryption key(s) which are used by the RP to encrypt requests to the server.
            var signedAlgorithm = (JwsAlg)Enum.Parse(typeof(JwsAlg), jwsProtectedHeader.alg);
            JsonWebKey jsonWebKey = null;

            var jsonWebKeys = _jsonWebKeyRepository.GetByAlgorithm(
                Use.Sig,
                signedAlgorithm.ToAllAlg(),
                new[] { KeyOperations.Sign });
            if (jsonWebKeys != null && jsonWebKeys.Any())
            {
                jsonWebKey = jsonWebKeys.First();
            }

            // If there's no algorithm available to sign the ID_TOKEN then don't secure the JWT
            if (jsonWebKey != null &&
                jwsProtectedHeader.alg.ToLowerInvariant() != "none")
            {
                jwsProtectedHeader.kid = jsonWebKey.Kid;
            }

            var jsonJwsProtectedHeader = jwsProtectedHeader.SerializeWithJavascript();
            var jwsProtectedHeaderBase64Encoded = jsonJwsProtectedHeader.Base64Encode();
            var jwsPayLoad = jwsPayload.SerializeWithJavascript();
            var jwsPayLoadBase64Encoded = jwsPayLoad.Base64Encode();
            var combinedProtectedHeaderAndPayLoad = string.Format("{0}.{1}", jwsProtectedHeaderBase64Encoded, jwsPayLoadBase64Encoded);            
            
            var signedJws = string.Empty;
            if (jsonWebKey != null)
            {
                switch (jsonWebKey.Kty)
                {
                    case KeyType.RSA:
                        signedJws = _createJwsSignature.SignWithRsa(signedAlgorithm, jsonWebKey.SerializedKey, combinedProtectedHeaderAndPayLoad);
                        break;
                }
            }

            return string.Format("{0}.{1}", combinedProtectedHeaderAndPayLoad, signedJws);
        }

        private JwsProtectedHeader ConstructProtectedHeader(Client client)
        {
            var signedAlgorithm = JwsAlg.RS256;
            if (!string.IsNullOrWhiteSpace(client.IdTokenSignedTResponseAlg))
            {
                signedAlgorithm = client.IdTokenSignedTResponseAlg.ToJwsAlg();
            }

            return new JwsProtectedHeader
            {
                alg = Enum.GetName(typeof(JwsAlg), signedAlgorithm),
                typ = JwsType
            };
        }

        private Dictionary<string, string> GetClaimsFromRequestedScopes(
            IEnumerable<string> scopes,
            ClaimsPrincipal claimsPrincipal)
        {
            var result = new Dictionary<string, string>
            {
                {
                    Constants.StandardResourceOwnerClaimNames.Subject, claimsPrincipal.GetSubject()
                }
            };
            foreach (var scope in scopes)
            {
                var scopeClaims = _scopeRepository.GetScopeByName(scope).Claims;
                if (scopeClaims == null)
                {
                    continue;
                }

                var openIdClaims = _claimsMapping.MapToOpenIdClaims(claimsPrincipal.Claims);
                openIdClaims.Where(oc => scopeClaims.Contains(oc.Key))
                    .Select(oc => new { key = oc.Key, val = oc.Value })
                    .ForEach(r => result.Add(r.key, r.val));
            }

            return result;
        } 
    }
}
