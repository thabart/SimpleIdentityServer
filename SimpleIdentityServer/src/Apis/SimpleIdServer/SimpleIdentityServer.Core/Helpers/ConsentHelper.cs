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

using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IConsentHelper
    {
        Task<Consent> GetConfirmedConsentsAsync(string subject, AuthorizationParameter authorizationParameter);
    }

    public class ConsentHelper : IConsentHelper
    {
        private readonly IConsentRepository _consentRepository;
        private readonly IParameterParserHelper _parameterParserHelper;

        public ConsentHelper(IConsentRepository consentRepository, IParameterParserHelper parameterParserHelper)
        {
            _consentRepository = consentRepository;
            _parameterParserHelper = parameterParserHelper;
        }

        public async Task<Consent> GetConfirmedConsentsAsync(string subject, AuthorizationParameter authorizationParameter)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var consents = await _consentRepository.GetConsentsForGivenUserAsync(subject);
            Consent confirmedConsent = null;
            if (consents != null && consents.Any())
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyUserInfoClaimParameter() ||
                    claimsParameter.IsAnyIdentityTokenClaimParameter())
                {
                    var expectedClaims = claimsParameter.GetClaimNames();
                    confirmedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.Claims != null && c.Claims.Any() &&
                            expectedClaims.Count() == c.Claims.Count() &&
                            expectedClaims.All(cl => c.Claims.Contains(cl)));
                }
                else
                {
                    var scopeNames =
                        _parameterParserHelper.ParseScopes(authorizationParameter.Scope);
                    confirmedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.GrantedScopes != null && c.GrantedScopes.Any() &&
                            scopeNames.Count() == c.GrantedScopes.Count() &&
                            c.GrantedScopes.All(g => scopeNames.Contains(g.Name)));
                }
            }

            return confirmedConsent;
        }
    }
}
