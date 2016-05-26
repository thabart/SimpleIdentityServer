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
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.Policies
{
    public interface IBasicAuthorizationPolicy
    {
        AuthorizationPolicyResult Execute(
            Ticket ticket,
            Policy policy,
            List<ClaimTokenParameter> claimTokenParameters);
    }

    internal class BasicAuthorizationPolicy : IBasicAuthorizationPolicy
    {
        private readonly IParametersProvider _parametersProvider;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private const string AccessTokenType = "http://openid.net/specs/openid-connect-core-1_0.html#HybridAccessToken2";

        #region Constructor

        public BasicAuthorizationPolicy(
            IParametersProvider parametersProvider,
            IIdentityServerClientFactory identityServerClientFactory)
        {
            _parametersProvider = parametersProvider;
            _identityServerClientFactory = identityServerClientFactory;
        }

        #endregion

        #region Public methods

        public AuthorizationPolicyResult Execute(
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

            // 2. Check the client is authorized
            if (authorizationPolicy.ClientIdsAllowed == null ||
                !authorizationPolicy.ClientIdsAllowed.Any() ||
                !authorizationPolicy.ClientIdsAllowed.Contains(validTicket.ClientId))
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.NotAuthorized
                };
            }

            // 3. Check the resource owner is authorized
            if (authorizationPolicy.Claims != null &&
                authorizationPolicy.Claims.Any())
            {
                if (claimTokenParameters == null ||
                    !claimTokenParameters.Any(c => c.Format == AccessTokenType))
                {
                    return GetNeedInfoResult(authorizationPolicy.Claims);
                }
            }

            // 4. Check the resource owner consent is needed
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
            var requiredClaims = new List<List<KeyValuePair<string, string>>>();
            foreach (var claim in claims)
            {
                requiredClaims.Add(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Constants.ErrorDetailNames.ClaimName, claim.Type),
                    new KeyValuePair<string, string>(Constants.ErrorDetailNames.ClaimFriendlyName, claim.Type),
                    new KeyValuePair<string, string>(Constants.ErrorDetailNames.ClaimIssuer, _parametersProvider.GetOpenIdConfigurationUrl())
                });
            }

            requestingPartyClaims.Add(Constants.ErrorDetailNames.RequiredClaims, requiredClaims);
            requestingPartyClaims.Add(Constants.ErrorDetailNames.RedirectUser, false);
            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.NeedInfo,
                ErrorDetails = new KeyValuePair<string, object>(Constants.ErrorDetailNames.RequestingPartyClaims, requestingPartyClaims)
            };
        }

        #endregion
    }
}
