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

using SimpleIdentityServer.Client;
using SimpleIdentityServer.Uma.Core.JwtToken;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Policies
{
    public interface IBasicAuthorizationPolicy
    {
        Task<AuthorizationPolicyResult> Execute(
            Ticket ticket,
            Policy policy,
            List<ClaimTokenParameter> claimTokenParameters);
    }

    internal class BasicAuthorizationPolicy : IBasicAuthorizationPolicy
    {
        private readonly IParametersProvider _parametersProvider;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly IJwtTokenParser _jwtTokenParser;

        private const string IdTokenType = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken";

        #region Constructor

        public BasicAuthorizationPolicy(
            IParametersProvider parametersProvider,
            IIdentityServerClientFactory identityServerClientFactory,
            IJwtTokenParser jwtTokenParser)
        {
            _parametersProvider = parametersProvider;
            _identityServerClientFactory = identityServerClientFactory;
            _jwtTokenParser = jwtTokenParser;
        }

        #endregion

        #region Public methods

        public async Task<AuthorizationPolicyResult> Execute(
            Ticket validTicket,
            Policy authorizationPolicy,
            List<ClaimTokenParameter> claimTokenParameters)
        {
            if (validTicket == null)
            {
                throw new ArgumentNullException(nameof(validTicket));
            }

            if (authorizationPolicy == null)
            {
                throw new ArgumentNullException(nameof(authorizationPolicy));
            }

            // 1. Check can access to the scope
            if (validTicket.Scopes.Any(s => !authorizationPolicy.Scopes.Contains(s)))
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                };
            }

            // 2. Check clients are correct
            var clientAuthorizationResult = CheckClients(authorizationPolicy, validTicket);
            if (clientAuthorizationResult != null &&
                clientAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized 
                && authorizationPolicy.AreConditionsLinked)
            {
                return clientAuthorizationResult;
            }

            // 3. Check claims are correct
            var claimAuthorizationResult = await CheckClaims(authorizationPolicy, claimTokenParameters);
            if (claimAuthorizationResult != null
                && claimAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized
                && authorizationPolicy.AreConditionsLinked)
            {
                return claimAuthorizationResult;
            }

            // 4. If both result are null then not authorized
            if (clientAuthorizationResult == null &&
                claimAuthorizationResult == null)
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                };
            }

            // 5. Check at least one condition is correct
            if (!(clientAuthorizationResult != null && clientAuthorizationResult.Type == AuthorizationPolicyResultEnum.Authorized ||
                claimAuthorizationResult != null && claimAuthorizationResult.Type == AuthorizationPolicyResultEnum.Authorized ||
                authorizationPolicy.AreConditionsLinked))
            {
                if (clientAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
                {
                    return clientAuthorizationResult;
                }

                return claimAuthorizationResult;
            }


            // 6. Check the resource owner consent is needed
            if (authorizationPolicy.IsResourceOwnerConsentNeeded && 
                !validTicket.IsAuthorizedByRo)
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.RequestSubmitted
                };
            }
            
            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.Authorized
            };
        }

        #endregion

        #region Private methods

        private AuthorizationPolicyResult GetNeedInfoResult(List<Claim> claims)
        {
            var requestingPartyClaims = new Dictionary<string, object>();
            var requiredClaims = new List<Dictionary<string, string>>();
            foreach (var claim in claims)
            {
                requiredClaims.Add(new Dictionary<string, string>
                {
                    {
                        Constants.ErrorDetailNames.ClaimName, claim.Type
                    },
                    {
                        Constants.ErrorDetailNames.ClaimFriendlyName, claim.Type
                    },
                    {
                        Constants.ErrorDetailNames.ClaimIssuer, _parametersProvider.GetOpenIdConfigurationUrl()
                    }
                });
            }

            requestingPartyClaims.Add(Constants.ErrorDetailNames.RequiredClaims, requiredClaims);
            requestingPartyClaims.Add(Constants.ErrorDetailNames.RedirectUser, false);
            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.NeedInfo,
                ErrorDetails = new Dictionary<string, object>
                {
                    {
                        Constants.ErrorDetailNames.RequestingPartyClaims,
                        requestingPartyClaims
                    }
                }
            };
        }

        private async Task<AuthorizationPolicyResult> CheckClaims(
            Policy authorizationPolicy,
            List<ClaimTokenParameter> claimTokenParameters)
        {
            if (authorizationPolicy.Claims == null ||
                !authorizationPolicy.Claims.Any())
            {
                return null;
            }


            if (claimTokenParameters == null ||
                !claimTokenParameters.Any(c => c.Format == IdTokenType))
            {
                return GetNeedInfoResult(authorizationPolicy.Claims);
            }

            var idToken = claimTokenParameters.First(c => c.Format == IdTokenType);
            var jwsPayload = await _jwtTokenParser.UnSign(idToken.Token);
            if (jwsPayload == null)
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                };
            }

            foreach (var claim in authorizationPolicy.Claims)
            {
                var payload = jwsPayload
                    .FirstOrDefault(j => j.Key == claim.Type);
                if (payload.Equals(default(KeyValuePair<string, object>)))
                {
                    return new AuthorizationPolicyResult
                    {
                        Type = AuthorizationPolicyResultEnum.NotAuthorized
                    };
                }

                if (claim.Type == SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role)
                {
                    if (!payload.Value.ToString().Split(',').Any(v => claim.Value == v))
                    {
                        return new AuthorizationPolicyResult
                        {
                            Type = AuthorizationPolicyResultEnum.NotAuthorized
                        };
                    }
                }
                else
                {
                    if (payload.Value.ToString() != claim.Value)
                    {
                        return new AuthorizationPolicyResult
                        {
                            Type = AuthorizationPolicyResultEnum.NotAuthorized
                        };
                    }
                }
            }

            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.Authorized
            };
        }

        private AuthorizationPolicyResult CheckClients(
            Policy authorizationPolicy,
            Ticket validTicket)
        {
            if (authorizationPolicy.ClientIdsAllowed == null ||
                !authorizationPolicy.ClientIdsAllowed.Any())
            {
                return null;
            }

            if (!authorizationPolicy.ClientIdsAllowed.Contains(validTicket.ClientId))
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                };
            }

            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.Authorized
            };
        }

        #endregion
    }
}
