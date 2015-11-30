using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using Microsoft.Practices.ObjectBuilder2;

using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Mapping;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Exceptions;

namespace SimpleIdentityServer.Core.JwtToken
{
    public interface IJwtGenerator
    {
        JwsPayload GenerateJwsPayload(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter);

        string Sign(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter);

        string Encrypt(
            string toEncrypt,
            AuthorizationParameter authorizationParameter);
    }

    public class JwtGenerator : IJwtGenerator
    {
        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IClientRepository _clientRepository;

        private readonly IClientValidator _clientValidator;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        private readonly IScopeRepository _scopeRepository;

        private readonly IClaimsMapping _claimsMapping;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IJwsGenerator _jwsGenerator;

        private readonly IJweGenerator _jweGenerator;

        public JwtGenerator(
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IClientRepository clientRepository,
            IClientValidator clientValidator,
            IJsonWebKeyRepository jsonWebKeyRepository,
            IScopeRepository scopeRepository,
            IClaimsMapping claimsMapping,
            IParameterParserHelper parameterParserHelper,
            IJwsGenerator jwsGenerator,
            IJweGenerator jweGenerator)
        {
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _scopeRepository = scopeRepository;
            _claimsMapping = claimsMapping;
            _parameterParserHelper = parameterParserHelper;
            _jwsGenerator = jwsGenerator;
            _jweGenerator = jweGenerator;
        }

        public JwsPayload GenerateJwsPayload(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var claimsParameter = authorizationParameter.Claims;
            if (claimsParameter == null ||
                claimsParameter.IdToken == null ||
                !claimsParameter.IdToken.Any())
            {
                return GenerateDefaultPayLoad(authorizationParameter, claimPrincipal);
            }

            return null;
        }

        public string Sign(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter)
        {
            var client = _clientRepository.GetClientById(authorizationParameter.ClientId);
            var jsonWebKey = GetSignJsonWebKey(client);
            var signedAlgorithm = GetJwsAlg(client);
            return _jwsGenerator.Generate(
                jwsPayload, 
                signedAlgorithm, 
                jsonWebKey);
        }

        public string Encrypt(
            string jwe,
            AuthorizationParameter authorizationParameter)
        {
            var client = _clientRepository.GetClientById(authorizationParameter.ClientId);
            var jsonWebKey = GetEncJsonWebKey(client);
            if (jsonWebKey == null)
            {
                return jwe;
            }

            var algEnum = GetJweAlg(client);
            var encEnum = GetJweEnc(client);

            return _jweGenerator.GenerateJwe(
                jwe, 
                algEnum, 
                encEnum, 
                jsonWebKey);
        }

        private JwsPayload GenerateDefaultPayLoad(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimPrincipal)
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
            var timeKeyValuePair = GetExpirationAndIssuedTime();
            var expirationInSeconds = timeKeyValuePair.Key;
            // Calculate the time in seconds which the JWT was issued.
            var iatInSeconds = timeKeyValuePair.Value;
            // Populate the claims
            var scopes = _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
            var claims = GetClaimsFromRequestedScopes(scopes, claimPrincipal);

            var result = new JwsPayload
            {
                {
                    Jwt.Constants.StandardClaimNames.Issuer, issuerName
                },
                {
                    Jwt.Constants.StandardClaimNames.Audiences, audiences.ToArray()
                },
                {
                    Jwt.Constants.StandardClaimNames.ExpirationTime, expirationInSeconds
                },
                {
                    Jwt.Constants.StandardClaimNames.Iat, iatInSeconds
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
                    result.Add(Jwt.Constants.StandardClaimNames.AuthenticationTime, double.Parse(authenticationInstant.Value));
                }
            }

            // Set the nonce value in the id token. The value is coming from the authorization request
            if (!string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                result.Add(Jwt.Constants.StandardClaimNames.Nonce, authorizationParameter.Nonce);
            }

            // Set the ACR : Authentication Context Class Reference
            // Set the AMR : Authentication Methods Reference
            // For the moment we support a level 1 because only password via HTTPS is supported.
            /*if (!string.IsNullOrWhiteSpace(authorizationParameter.AcrValues))
            {*/
            result.Add(Jwt.Constants.StandardClaimNames.Acr, Constants.StandardArcParameterNames.OpenIdCustomAuthLevel + ".password=1");
            result.Add(Jwt.Constants.StandardClaimNames.Amr, "password");
            //}

            // Set the client_id
            // This claim is only needed when the ID token has a single audience value & that audience is different than the authorized party.
            if (audiences != null &&
                audiences.Count() == 1 &&
                audiences.First() == authorizationParameter.ClientId)
            {
                result.Add(Jwt.Constants.StandardClaimNames.Azp, authorizationParameter.ClientId);
            }

            // TODO : Add another claims in it ...

            return result;
        }

        private JwsPayload GenerateFilteredPayLoad(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            List<ClaimParameter> claimParameters)
        {
            var result = new JwsPayload();
            var issuerClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Issuer);
            var audiencesClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Audiences);
            var expirationTimeClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.ExpirationTime);
            var issuedAtTimeClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Iat);
            var resourceOwnerClaimParameters = claimParameters.Where(c => Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(c.Name));
            
            // Fill-in the issuer
            if (issuerClaimParameter != null)
            {
                // Get the issuer from the configuration.
                var issuerName = _simpleIdentityServerConfigurator.GetIssuerName();
                var issuerIsValid = ValidateClaimValue(issuerName, issuerClaimParameter);
                if (!issuerIsValid)
                {
                    // TODO : throw an exception
                }

                result.Add(Jwt.Constants.StandardClaimNames.Issuer, issuerName);
            }

            // Fill-in the audiences
            if (audiencesClaimParameter != null)
            {
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

                var audiencesIsValid = ValidateClaimValues(audiences.ToArray(), audiencesClaimParameter);
                if (!audiencesIsValid)
                {
                    // TODO : throw an exception
                }

                result.Add(Jwt.Constants.StandardClaimNames.Audiences, audiences);
            }
            
            var timeKeyValuePair = GetExpirationAndIssuedTime();
            // Fill-in the expiration time
            if (expirationTimeClaimParameter != null)
            {
                // Calculate the expiration datetime
                var expirationInSeconds = timeKeyValuePair.Key;
                var expirationInSecondsIsValid = ValidateClaimValue(expirationInSeconds.ToString(), expirationTimeClaimParameter);
                if (!expirationInSecondsIsValid)
                {
                    // TODO : throw an exception
                }

                result.Add(Jwt.Constants.StandardClaimNames.ExpirationTime, expirationInSeconds);
            }

            // Fill-in the issued at time
            if (issuedAtTimeClaimParameter != null)
            {
                var issuedAtTime = timeKeyValuePair.Value;
                var issuedAtTimeIsValid = ValidateClaimValue(issuedAtTime.ToString(), issuedAtTimeClaimParameter);
                if (!issuedAtTimeIsValid)
                {
                    // TODO : throw an exception
                }

                result.Add(Jwt.Constants.StandardClaimNames.Iat, issuedAtTime);
            }

            if (resourceOwnerClaimParameters != null)
            {
                var requestedClaimNames = resourceOwnerClaimParameters.Select(r => r.Name);
                var resourceOwnerClaims = GetClaims(requestedClaimNames, claimsPrincipal);
                foreach(var resourceOwnerClaimParameter in resourceOwnerClaimParameters)
                {
                    var resourceOwnerClaim = resourceOwnerClaims.FirstOrDefault(c => c.Key == resourceOwnerClaimParameter.Name);
                    if (resourceOwnerClaim.Equals(typeof(KeyValuePair<string, string>)))
                    {
                        // TODO : throw an exception
                    }

                    var isClaimValid = ValidateClaimValue(resourceOwnerClaim.Value, resourceOwnerClaimParameter);
                    if (!isClaimValid)
                    {
                        // TODO : throw an exception
                    }
                }
            }

            // resourceOwnerClaimParameters

            return null;
        }

        private bool ValidateClaimValue(
            string claimValue,
            ClaimParameter claimParameter)
        {
            if (claimParameter.EssentialParameterExist &&
                string.IsNullOrWhiteSpace(claimValue) &&
                claimParameter.Essential)
            {
                return false;                  
            }

            if (claimParameter.ValuesParameterExist && 
                claimValue != claimParameter.Value)
            {
                return false;
            }

            if (claimParameter.ValuesParameterExist &&
                claimParameter.Values != null &&
                claimParameter.Values.Contains(claimValue))
            {
                return false;
            }

            return true;
        }

        private bool ValidateClaimValues(
            string[] claimValues,
            ClaimParameter claimParameter)
        {
            if (claimParameter.EssentialParameterExist && 
                (claimValues == null || claimValues.Any()) 
                && claimParameter.Essential)
            {
                return false;
            }

            if (claimParameter.ValueParameterExist && 
                claimValues.Contains(claimParameter.Value))
            {
                return false;
            }

            if (claimParameter.ValuesParameterExist &&
                claimParameter.Values != null &&
                claimParameter.Values.All(c => claimValues.Contains(c)))
            {
                return false;
            }

            return true;
        }

        private Dictionary<string, string> GetClaimsFromRequestedScopes(
            IEnumerable<string> scopes,
            ClaimsPrincipal claimsPrincipal)
        {
            var result = new Dictionary<string, string>
            {
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Subject, claimsPrincipal.GetSubject()
                }
            };

            foreach (var scope in scopes)
            {
                var scopeClaims = _scopeRepository.GetScopeByName(scope).Claims;
                if (scopeClaims == null)
                {
                    continue;
                }

                result.AddRange(GetClaims(scopeClaims, claimsPrincipal));
            }

            return result;
        }

        private Dictionary<string, string> GetClaims(
            IEnumerable<string> claims,
            ClaimsPrincipal claimsPrincipal)
        {
            var result = new Dictionary<string, string>();
            var openIdClaims = _claimsMapping.MapToOpenIdClaims(claimsPrincipal.Claims);
            openIdClaims.Where(oc => claims.Contains(oc.Key))
                .Select(oc => new { key = oc.Key, val = oc.Value })
                .ForEach(r => result.Add(r.key, r.val));
            return result;
        }

        private JsonWebKey GetEncJsonWebKey(Client client)
        {
            var algName = client.IdTokenEncryptedResponseAlg;
            if (string.IsNullOrWhiteSpace(algName) ||
                !Jwt.Constants.MappingNameToJweAlgEnum.Keys.Contains(algName))
            {
                return null;
            }

            var encEnum = GetJweEnc(client);
            var algEnum = GetJweAlg(client);

            return GetJsonWebKey(
                algEnum.ToAllAlg(),
                KeyOperations.Encrypt,
                Use.Enc);
        }

        private JsonWebKey GetSignJsonWebKey(Client client)
        {
            var signedAlgorithm = GetJwsAlg(client);

            // In the "open-id-connect-discovery" there's an endpoint jwks_uri :
            // This url contains the signing key's) the RP uses to validate signatures from the OP
            // The JWS set may also contain the Server's encryption key(s) which are used by the RP to encrypt requests to the server.
            return GetJsonWebKey(
                signedAlgorithm.ToAllAlg(),
                KeyOperations.Sign,
                Use.Sig);
        }

        private JweEnc GetJweEnc(Client client)
        {
            var encName = client.IdTokenEncryptedResponseEnc;
            JweEnc encEnum;
            if (string.IsNullOrWhiteSpace(encName) ||
                !Jwt.Constants.MappingNameToJweEncEnum.Keys.Contains(encName))
            {
                encEnum = JweEnc.A128CBC_HS256;
            }
            else
            {
                encEnum = Jwt.Constants.MappingNameToJweEncEnum[encName];
            }

            return encEnum;
        }

        private JweAlg GetJweAlg(Client client)
        {
            var algName = client.IdTokenEncryptedResponseAlg;
            return Jwt.Constants.MappingNameToJweAlgEnum[algName];
        }

        private JwsAlg GetJwsAlg(Client client)
        {
            var signedAlg = client.IdTokenSignedTResponseAlg;
            JwsAlg signedAlgorithm;
            if (string.IsNullOrWhiteSpace(signedAlg)
                || !Enum.TryParse(signedAlg, out signedAlgorithm))
            {
                signedAlgorithm = JwsAlg.HS256;
            }

            return signedAlgorithm;
        }

        private JsonWebKey GetJsonWebKey(
            AllAlg alg,
            KeyOperations operation,
            Use use)
        {
            JsonWebKey result = null;
            var jsonWebKeys = _jsonWebKeyRepository.GetByAlgorithm(
                use,
                alg,
                new[] { operation });
            if (jsonWebKeys != null && jsonWebKeys.Any())
            {
                result = jsonWebKeys.First();
            }

            return result;
        }

        private KeyValuePair<double, double> GetExpirationAndIssuedTime()
        {
            var currentDateTime = DateTime.UtcNow;
            var expiredDateTime = currentDateTime.AddSeconds(_simpleIdentityServerConfigurator.GetTokenValidityPeriodInSeconds());
            var expirationInSeconds = expiredDateTime.ConvertToUnixTimestamp();
            var iatInSeconds = currentDateTime.ConvertToUnixTimestamp();
            return new KeyValuePair<double, double>(expirationInSeconds, iatInSeconds);
        }
    }
}
