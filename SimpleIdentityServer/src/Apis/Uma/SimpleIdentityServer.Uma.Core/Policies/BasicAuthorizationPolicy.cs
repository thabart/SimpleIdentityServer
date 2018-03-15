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

using Newtonsoft.Json.Linq;
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
        Task<AuthorizationPolicyResult> Execute(TicketLineParameter ticket, Policy policy, ClaimTokenParameter claimTokenParameters);
    }

    internal class BasicAuthorizationPolicy : IBasicAuthorizationPolicy
    {
        private readonly IParametersProvider _parametersProvider;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IJwtTokenParser _jwtTokenParser;
        
        public BasicAuthorizationPolicy(IParametersProvider parametersProvider, IIdentityServerClientFactory identityServerClientFactory, IJwtTokenParser jwtTokenParser)
        {
            _parametersProvider = parametersProvider;
            _identityServerClientFactory = identityServerClientFactory;
            _jwtTokenParser = jwtTokenParser;
        }

        #region Public methods

        public async Task<AuthorizationPolicyResult> Execute(TicketLineParameter ticketLineParameter, Policy authorizationPolicy, ClaimTokenParameter claimTokenParameter)
        {
            if (ticketLineParameter == null)
            {
                throw new ArgumentNullException(nameof(ticketLineParameter));
            }

            if (authorizationPolicy == null)
            {
                throw new ArgumentNullException(nameof(authorizationPolicy));
            }

            if (authorizationPolicy.Rules == null ||
                !authorizationPolicy.Rules.Any())
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized
                };
            }

            AuthorizationPolicyResult result = null;
            foreach (var rule in authorizationPolicy.Rules)
            {
                result = await ExecuteAuthorizationPolicyRule(ticketLineParameter, rule, claimTokenParameter);
                if (result.Type == AuthorizationPolicyResultEnum.Authorized)
                {
                    return result;
                }
            }

            return result;
        }

        #endregion

        #region Private methods

        private async Task<AuthorizationPolicyResult> ExecuteAuthorizationPolicyRule(TicketLineParameter ticketLineParameter, PolicyRule authorizationPolicy, ClaimTokenParameter claimTokenParameter)
        {
            // 1. Check can access to the scope
            if (ticketLineParameter.Scopes.Any(s => !authorizationPolicy.Scopes.Contains(s)))
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                };
            }

            // 2. Check clients are correct
            var clientAuthorizationResult = CheckClients(authorizationPolicy, ticketLineParameter);
            if (clientAuthorizationResult != null &&
                clientAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                return clientAuthorizationResult;
            }

            // 3. Check claims are correct
            var claimAuthorizationResult = await CheckClaims(authorizationPolicy, claimTokenParameter);
            if (claimAuthorizationResult != null
                && claimAuthorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                return claimAuthorizationResult;
            }

            // 4. Check the resource owner consent is needed
            if (authorizationPolicy.IsResourceOwnerConsentNeeded && !ticketLineParameter.IsAuthorizedByRo)
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

        private async Task<AuthorizationPolicyResult> CheckClaims(PolicyRule authorizationPolicy, ClaimTokenParameter claimTokenParameter)
        {
            if (authorizationPolicy.Claims == null ||
                !authorizationPolicy.Claims.Any())
            {
                return null;
            }

            if (claimTokenParameter == null || claimTokenParameter.Format != Constants.IdTokenType)
            {
                return GetNeedInfoResult(authorizationPolicy.Claims);
            }

            var idToken = claimTokenParameter.Token;
            var jwsPayload = await _jwtTokenParser.UnSign(idToken, authorizationPolicy.OpenIdProvider);
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
                    IEnumerable<string> roles = null;
                    if (payload.Value is string)
                    {
                        roles = payload.Value.ToString().Split(',');
                    }
                    else
                    {
                        var arr = payload.Value as object[];
                        var jArr = payload.Value as JArray;
                        if (arr != null)
                        {
                            roles = arr.Select(c => c.ToString());
                        }

                        if (jArr != null)
                        {
                            roles = jArr.Select(c => c.ToString());
                        }
                    }

                    if (roles == null || !roles.Any(v => claim.Value == v))
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

        private AuthorizationPolicyResult CheckClients(PolicyRule authorizationPolicy, TicketLineParameter ticketLineParameter)
        {
            if (authorizationPolicy.ClientIdsAllowed == null ||
                !authorizationPolicy.ClientIdsAllowed.Any())
            {
                return null;
            }

            if (!authorizationPolicy.ClientIdsAllowed.Contains(ticketLineParameter.ClientId))
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
