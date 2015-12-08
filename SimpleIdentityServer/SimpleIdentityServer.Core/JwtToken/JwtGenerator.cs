using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using Microsoft.Practices.ObjectBuilder2;

using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
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
        JwsPayload GenerateIdTokenPayloadForScopes(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter);

        JwsPayload GenerateFilteredIdTokenPayload(
           ClaimsPrincipal claimsPrincipal,
           AuthorizationParameter authorizationParameter,
           List<ClaimParameter> claimParameters);

        JwsPayload GenerateUserInfoPayloadForScope(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter);

        JwsPayload GenerateFilteredUserInfoPayload(
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter);

        string Sign(
            JwsPayload jwsPayload,
            string clientId);

        string Encrypt(
            string toEncrypt,
            string clientId);
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

        #region Public methods

        public JwsPayload GenerateIdTokenPayloadForScopes(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var result = new JwsPayload();
            PopulateSubject(result, claimPrincipal, new List<ClaimParameter>(), authorizationParameter);
            PopulateIdentityToken(result, authorizationParameter, new List<ClaimParameter>(), claimPrincipal);
            PopulateClaimsForScopes(result, authorizationParameter, claimPrincipal);
            return result;
        }

        public JwsPayload GenerateFilteredIdTokenPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            List<ClaimParameter> claimParameters)
        {
            var result = new JwsPayload();
            PopulateSubject(result, claimsPrincipal, claimParameters, authorizationParameter);
            PopulateIdentityToken(result, authorizationParameter, claimParameters, claimsPrincipal);
            PopulateIndividualClaims(result, claimParameters, claimsPrincipal, authorizationParameter);
            return result;
        }

        public JwsPayload GenerateUserInfoPayloadForScope(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var result = new JwsPayload();
            PopulateSubject(result, claimsPrincipal, new List<ClaimParameter>(), authorizationParameter);
            PopulateClaimsForScopes(result, authorizationParameter, claimsPrincipal);
            return result;
        }

        public JwsPayload GenerateFilteredUserInfoPayload(List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var result = new JwsPayload();
            PopulateSubject(result, claimsPrincipal, claimParameters, authorizationParameter);
            PopulateIndividualClaims(result, claimParameters, claimsPrincipal, authorizationParameter);
            return result;
        }

        public string Sign(
            JwsPayload jwsPayload,
            string clientId)
        {
            var client = _clientRepository.GetClientById(clientId);
            var jsonWebKey = GetSignJsonWebKey(client);
            var signedAlgorithm = GetJwsAlg(client);
            return _jwsGenerator.Generate(
                jwsPayload, 
                signedAlgorithm, 
                jsonWebKey);
        }

        public string Encrypt(
            string jwe,
            string clientId)
        {
            var client = _clientRepository.GetClientById(clientId);
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

        #endregion

        #region Private methods

        private void PopulateSubject(
            JwsPayload jwsPayload,
            ClaimsPrincipal claimsPrincipal,
            IEnumerable<ClaimParameter> claimParameters,
            AuthorizationParameter authorizationParameter)
        {
            var state = authorizationParameter == null ? string.Empty : authorizationParameter.State;
            var subject = claimsPrincipal.GetSubject();
            var subjectClaimParameter =
                claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            if (subjectClaimParameter != null)
            {
                var subjectIsValid = ValidateClaimValue(subject, subjectClaimParameter);
                if (!subjectIsValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardResourceOwnerClaimNames.Subject),
                        state);
                }
            }

            jwsPayload.Add(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject);
        }

        private void PopulateClaimsForScopes(
            JwsPayload result,
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            var scope = authorizationParameter.Scope;
            if (string.IsNullOrWhiteSpace(scope))
            {
                return;
            }

            var scopes = _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
            var claims = GetClaimsFromRequestedScopes(scopes, claimsPrincipal);
            foreach (var claim in claims)
            {
                result.Add(claim.Key, claim.Value);
            }
        }

        private void PopulateIndividualClaims(
            JwsPayload result,
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var resourceOwnerClaimParameters = claimParameters.Where(c => Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(c.Name));
            if (resourceOwnerClaimParameters != null)
            {
                var requestedClaimNames = resourceOwnerClaimParameters.Select(r => r.Name);
                var resourceOwnerClaims = GetClaims(requestedClaimNames, claimsPrincipal);
                foreach (var resourceOwnerClaimParameter in resourceOwnerClaimParameters)
                {
                    var resourceOwnerClaim = resourceOwnerClaims.FirstOrDefault(c => c.Key == resourceOwnerClaimParameter.Name);
                    if (resourceOwnerClaim.Equals(typeof(KeyValuePair<string, string>)))
                    {
                        throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                            string.Format(ErrorDescriptions.TheClaimIsNotValid, resourceOwnerClaim.Key),
                            authorizationParameter.State);
                    }

                    var isClaimValid = ValidateClaimValue(resourceOwnerClaim.Value, resourceOwnerClaimParameter);
                    if (!isClaimValid)
                    {
                        throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                            string.Format(ErrorDescriptions.TheClaimIsNotValid, resourceOwnerClaim.Key),
                            authorizationParameter.State);
                    }

                    result.Add(resourceOwnerClaim.Key, resourceOwnerClaim.Value);
                }
            }
        }

        private void PopulateIdentityToken(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter,
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal)
        {
            var nonce = authorizationParameter == null ? string.Empty : authorizationParameter.Nonce;
            var state = authorizationParameter == null ? string.Empty : authorizationParameter.State;
            var clientId = authorizationParameter == null ? string.Empty : authorizationParameter.ClientId;
            var maxAge = authorizationParameter == null ? default(double) : authorizationParameter.MaxAge;

            var issuerClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Issuer);
            var audiencesClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Audiences);
            var expirationTimeClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.ExpirationTime);
            var issuedAtTimeClaimParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Iat);
            var authenticationTimeParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.AuthenticationTime);
            var nonceParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Nonce);
            var acrParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Acr);
            var amrParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Amr);
            var azpParameter = claimParameters.FirstOrDefault(c => c.Name == Jwt.Constants.StandardClaimNames.Azp);

            var timeKeyValuePair = GetExpirationAndIssuedTime();
            var issuerName = _simpleIdentityServerConfigurator.GetIssuerName();
            var audiences = new List<string>();
            var expirationInSeconds = timeKeyValuePair.Key;
            var issuedAtTime = timeKeyValuePair.Value;
            var acrValues = Constants.StandardArcParameterNames.OpenIdCustomAuthLevel + ".password=1";
            const string amr = "password";
            var azp = string.Empty;

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

            // The identity token can be reused by the simple identity server.
            audiences.Add(_simpleIdentityServerConfigurator.GetIssuerName());

            var authenticationInstant = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
            var authenticationInstantValue = authenticationInstant == null
                ? string.Empty
                : authenticationInstant.Value;

            if (issuerClaimParameter != null)
            {
                var issuerIsValid = ValidateClaimValue(issuerName, issuerClaimParameter);
                if (!issuerIsValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Issuer),
                        state);
                }
            }

            if (audiences != null &&
                audiences.Count() > 1)
            {
                azp = authorizationParameter.ClientId;
            }

            if (audiencesClaimParameter != null)
            {
                var audiencesIsValid = ValidateClaimValues(audiences.ToArray(), audiencesClaimParameter);
                if (!audiencesIsValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Audiences),
                        state);
                }
            }

            if (expirationTimeClaimParameter != null)
            {
                var expirationInSecondsIsValid = ValidateClaimValue(expirationInSeconds.ToString(), expirationTimeClaimParameter);
                if (!expirationInSecondsIsValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.ExpirationTime),
                        state);
                }
            }

            if (issuedAtTimeClaimParameter != null)
            {
                var issuedAtTimeIsValid = ValidateClaimValue(issuedAtTime.ToString(), issuedAtTimeClaimParameter);
                if (!issuedAtTimeIsValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Iat),
                        state);
                }
            }

            if (authenticationTimeParameter != null)
            {
                var isAuthenticationTimeValid = ValidateClaimValue(authenticationInstantValue, authenticationTimeParameter);
                if (!isAuthenticationTimeValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.AuthenticationTime),
                        state);
                }
            }

            if (acrParameter != null)
            {
                var isAcrParameterValid = ValidateClaimValue(acrValues, acrParameter);
                if (!isAcrParameterValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Acr),
                        state);
                }
            }

            if (nonceParameter != null)
            {
                var isNonceParameterValid = ValidateClaimValue(nonce, nonceParameter);
                if (!isNonceParameterValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Nonce),
                        state);
                }
            }

            if (amrParameter != null)
            {
                var isAmrParameterValid = ValidateClaimValue(amr, amrParameter);
                if (!isAmrParameterValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Amr),
                        state);
                }
            }

            // Fill-in the AZP parameter
            if (azpParameter != null)
            {
                var isAzpParameterValid = ValidateClaimValue(azp, azpParameter);
                if (!isAzpParameterValid)
                {
                    throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, Jwt.Constants.StandardClaimNames.Azp),
                        state);
                }
            }

            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Issuer, issuerName);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Audiences, audiences);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.ExpirationTime, expirationInSeconds);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Iat, issuedAtTime);
            // Set the auth_time if it's requested as an essential claim OR the max_age request is specified
            if (((authenticationTimeParameter != null && authenticationTimeParameter.Essential) ||
                !maxAge.Equals(default(double))) && !string.IsNullOrWhiteSpace(authenticationInstantValue))
            {
                jwsPayload.Add(Jwt.Constants.StandardClaimNames.AuthenticationTime, double.Parse(authenticationInstantValue));
            }

            if (!string.IsNullOrWhiteSpace(nonce))
            {
                jwsPayload.Add(Jwt.Constants.StandardClaimNames.Nonce, nonce);
            }

            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Acr, acrValues);
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Amr, amr);
            if (!string.IsNullOrWhiteSpace(azp))
            {
                jwsPayload.Add(Jwt.Constants.StandardClaimNames.Azp, azp);
            }
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

            if (claimParameter.ValueParameterExist && 
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
            var result = new Dictionary<string, string>();
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
            var currentDateTime = DateTimeOffset.UtcNow;
            var expiredDateTime = currentDateTime.AddSeconds(_simpleIdentityServerConfigurator.GetTokenValidityPeriodInSeconds());
            var expirationInSeconds = expiredDateTime.ConvertToUnixTimestamp();
            var iatInSeconds = currentDateTime.ConvertToUnixTimestamp();
            return new KeyValuePair<double, double>(expirationInSeconds, iatInSeconds);
        }

        #endregion
    }
}
