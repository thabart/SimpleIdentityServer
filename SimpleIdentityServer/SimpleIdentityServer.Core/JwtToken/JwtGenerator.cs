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
using System.Collections.Generic;
using System.Globalization;
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
using System.Security.Cryptography;
using System.Text;
using SimpleIdentityServer.Core.Common.Extensions;

namespace SimpleIdentityServer.Core.JwtToken
{
    public interface IJwtGenerator
    {
        JwsPayload GenerateIdTokenPayloadForScopes(
            ClaimsPrincipal claimsPrincipal,
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

        void FillInOtherClaimsIdentityTokenPayload(JwsPayload jwsPayload,
            string authorizationCode,
            string accessToken,
            AuthorizationParameter authorizationParameter);

        string Sign(
            JwsPayload jwsPayload,
            JwsAlg alg);

        string Encrypt(
            string jwe,
            JweAlg jweAlg,
            JweEnc jweEnc);
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

        private readonly Dictionary<JwsAlg, Func<string, string>> _mappingJwsAlgToHashingFunctions = new Dictionary<JwsAlg, Func<string, string>>
        {
            {
                JwsAlg.ES256, HashWithSha256
            },
            {
                JwsAlg.ES384, HashWithSha384
            },
            {
                JwsAlg.ES512, HashWithSha512
            },
            {
                JwsAlg.HS256, HashWithSha256
            },
            {
                JwsAlg.HS384, HashWithSha384
            },
            {
                JwsAlg.HS512, HashWithSha512
            },
            {
                JwsAlg.PS256, HashWithSha256
            },
            {
                JwsAlg.PS384, HashWithSha384
            },
            {
                JwsAlg.PS512, HashWithSha512
            },
            {
                JwsAlg.RS256, HashWithSha256
            },
            {
                JwsAlg.RS384, HashWithSha384
            },
            {
                JwsAlg.RS512, HashWithSha512
            }
        };

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
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null ||
                !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException("claimsPrincipal");
            }

            var result = new JwsPayload();
            FillInIdentityTokenClaims(result, authorizationParameter, new List<ClaimParameter>(), claimsPrincipal);
            FillInResourceOwnerClaimsFromScopes(result, authorizationParameter, claimsPrincipal);
            return result;
        }

        public JwsPayload GenerateFilteredIdTokenPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            List<ClaimParameter> claimParameters)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null ||
                !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException("claimsPrincipal");
            }

            var result = new JwsPayload();
            FillInIdentityTokenClaims(result, authorizationParameter, claimParameters, claimsPrincipal);
            FillInResourceOwnerClaimsByClaimsParameter(result, 
                claimParameters, 
                claimsPrincipal, 
                authorizationParameter);
            return result;
        }

        public JwsPayload GenerateUserInfoPayloadForScope(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null ||
                !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException("claimsPrincipal");
            }
            
            var result = new JwsPayload();
            FillInResourceOwnerClaimsFromScopes(result, authorizationParameter, claimsPrincipal);
            return result;
        }

        public JwsPayload GenerateFilteredUserInfoPayload(List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null ||
                !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException("claimsPrincipal");
            }

            var result = new JwsPayload();
            FillInResourceOwnerClaimsByClaimsParameter(result, claimParameters, claimsPrincipal, authorizationParameter);
            return result;
        }

        public void FillInOtherClaimsIdentityTokenPayload(JwsPayload jwsPayload,
            string authorizationCode,
            string accessToken,
            AuthorizationParameter authorizationParameter)
        {
            if (jwsPayload == null)
            {
                throw new ArgumentNullException("jwsPayload");
            }

            var client = _clientValidator.ValidateClientExist(authorizationParameter.ClientId);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format("the client id {0} doesn't exist", 
                    authorizationParameter.ClientId));
            }

            var signedAlg = client.GetIdTokenSignedResponseAlg();
            if (signedAlg == null || 
                signedAlg == JwsAlg.none)
            {
                return;
            }

            if (!_mappingJwsAlgToHashingFunctions.ContainsKey(signedAlg.Value))
            {
                throw new InvalidOperationException(string.Format("the alg {0} is not supported",
                    signedAlg.Value));
            }

            var callback = _mappingJwsAlgToHashingFunctions[signedAlg.Value];
            if (!string.IsNullOrWhiteSpace(authorizationCode))
            {
                var hashingAuthorizationCode = callback(authorizationCode);
                jwsPayload.Add(Jwt.Constants.StandardClaimNames.CHash, hashingAuthorizationCode);
            }
            
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var hashingAccessToken = callback(accessToken);
                jwsPayload.Add(Jwt.Constants.StandardClaimNames.AtHash, hashingAccessToken);
            }
        }

        public string Sign(
            JwsPayload jwsPayload,
            JwsAlg alg)
        {
            var jsonWebKey = GetJsonWebKey(
                alg.ToAllAlg(),
                KeyOperations.Sign,
                Use.Sig);
            return _jwsGenerator.Generate(
                jwsPayload,
                alg, 
                jsonWebKey);
        }

        public string Encrypt(
            string jwe,
            JweAlg jweAlg,
            JweEnc jweEnc)
        {
            var jsonWebKey = GetJsonWebKey(
                jweAlg.ToAllAlg(),
                KeyOperations.Encrypt,
                Use.Enc);
            if (jsonWebKey == null)
            {
                return jwe;
            }

            return _jweGenerator.GenerateJwe(
                jwe, 
                jweAlg, 
                jweEnc, 
                jsonWebKey);
        }

        #endregion

        #region Private methods

        private void FillInResourceOwnerClaimsFromScopes(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            // 1. Fill-in the subject claim
            var subject = claimsPrincipal.GetSubject();
            jwsPayload.Add(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject);

            if (authorizationParameter == null ||
                string.IsNullOrWhiteSpace(authorizationParameter.Scope))
            {
                return;
            }

            // 2. Fill-in the other claims
            var scopes = _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
            var claims = GetClaimsFromRequestedScopes(scopes, claimsPrincipal);
            foreach (var claim in claims)
            {
                if (claim.Key == Jwt.Constants.StandardResourceOwnerClaimNames.Subject)
                {
                    continue;
                }

                jwsPayload.Add(claim.Key, claim.Value);
            }
        }

        private void FillInResourceOwnerClaimsByClaimsParameter(
            JwsPayload jwsPayload,
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var state = authorizationParameter == null ? string.Empty : authorizationParameter.State;

            // 1. Fill-In the subject - set the subject as an essential claim
            if (claimParameters.All(c => c.Name != Jwt.Constants.StandardResourceOwnerClaimNames.Subject))
            {
                var essentialSubjectClaimParameter = new ClaimParameter
                {
                    Name = Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            Constants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                };

                claimParameters.Add(essentialSubjectClaimParameter);
            }

            // 2. Fill-In all the other resource owner claims
            if (claimParameters == null ||
                !claimParameters.Any())
            {
                return;
            }

            var resourceOwnerClaimParameters = claimParameters
                .Where(c => Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(c.Name))
                .ToList();
            if (resourceOwnerClaimParameters.Any())
            {
                var requestedClaimNames = resourceOwnerClaimParameters.Select(r => r.Name);
                var resourceOwnerClaims = GetClaims(requestedClaimNames, claimsPrincipal);
                foreach (var resourceOwnerClaimParameter in resourceOwnerClaimParameters)
                {
                    var resourceOwnerClaim = resourceOwnerClaims.FirstOrDefault(c => c.Key == resourceOwnerClaimParameter.Name);
                    var resourceOwnerClaimValue = resourceOwnerClaim.Equals(default(KeyValuePair<string, string>)) ? string.Empty : resourceOwnerClaim.Value;
                    var isClaimValid = ValidateClaimValue(resourceOwnerClaimValue, resourceOwnerClaimParameter);
                    if (!isClaimValid)
                    {
                        throw new IdentityServerExceptionWithState(ErrorCodes.InvalidGrant,
                            string.Format(ErrorDescriptions.TheClaimIsNotValid, resourceOwnerClaimParameter.Name),
                            state);
                    }

                    jwsPayload.Add(resourceOwnerClaim.Key, resourceOwnerClaim.Value);
                }
            }
        }

        private void FillInIdentityTokenClaims(
            JwsPayload jwsPayload,
            AuthorizationParameter authorizationParameter,
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal)
        {
            var nonce = authorizationParameter.Nonce;
            var state = authorizationParameter.State;
            var clientId = authorizationParameter.ClientId;
            var maxAge = authorizationParameter.MaxAge;

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
            var amr = new []{ "password" };
            var azp = string.Empty;

            var clients = _clientRepository.GetAll();
            clients.ForEach(client =>
            {
                var isClientSupportIdTokenResponseType =
                    _clientValidator.ValidateResponseType(ResponseType.id_token, client);
                if (isClientSupportIdTokenResponseType ||
                    client.ClientId == authorizationParameter.ClientId)
                {
                    audiences.Add(client.ClientId);
                }
            });

            // The identity token can be reused by the simple identity server.
            if (!string.IsNullOrWhiteSpace(issuerName))
            {
                audiences.Add(issuerName);
            }

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

            if (audiences.Count() > 1 ||
                audiences.Count() == 1 &&
                audiences.First() !=  clientId)
            {
                azp = clientId;
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
                var expirationInSecondsIsValid = ValidateClaimValue(expirationInSeconds.ToString(CultureInfo.InvariantCulture), expirationTimeClaimParameter);
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
                var isAmrParameterValid = ValidateClaimValues(amr, amrParameter);
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
            jwsPayload.Add(Jwt.Constants.StandardClaimNames.Audiences, audiences.ToArray());
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
                (claimValues == null || !claimValues.Contains(claimParameter.Value)))
            {
                return false;
            }

            if (claimParameter.ValuesParameterExist &&
                claimParameter.Values != null &&
                (claimValues == null || !claimParameter.Values.All(c => claimValues.Contains(c))))
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

        #region Private static methods

        private static string HashWithSha256(string parameter)
        {
            var sha256 = SHA256Managed.Create();
            return GetFirstPart(parameter,
                sha256);
        }

        private static string HashWithSha384(string parameter)
        {
            var sha384 = SHA384.Create();
            return GetFirstPart(parameter,
                sha384);
        }

        private static string HashWithSha512(string parameter)
        {
            var sha512 = SHA512.Create();
            return GetFirstPart(parameter,
                sha512);
        }

        private static string GetFirstPart(string parameter,
            HashAlgorithm alg)
        {
            var hashingResultBytes = alg.ComputeHash(Encoding.UTF8.GetBytes(parameter));
            var split = ByteManipulator.SplitByteArrayInHalf(hashingResultBytes);
            var firstPart = split[0];
            return firstPart.Base64EncodeBytes();
        }

        #endregion
    }
}
