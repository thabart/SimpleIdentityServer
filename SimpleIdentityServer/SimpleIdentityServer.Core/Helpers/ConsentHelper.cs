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

using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Linq;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IConsentHelper
    {
        Consent GetConsentConfirmedByResourceOwner(
            string subject,
            AuthorizationParameter authorizationParameter);
    }

    public class ConsentHelper : IConsentHelper
    {
        private readonly IConsentRepository _consentRepository;

        private readonly IParameterParserHelper _parameterParserHelper;

        public ConsentHelper(
            IConsentRepository consentRepository,
            IParameterParserHelper parameterParserHelper)
        {
            _consentRepository = consentRepository;
            _parameterParserHelper = parameterParserHelper;
        }

        public Consent GetConsentConfirmedByResourceOwner(
            string subject,
            AuthorizationParameter authorizationParameter)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            var consents = _consentRepository.GetConsentsForGivenUser(subject);
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
                            expectedClaims.All(cl => c.Claims.Contains(cl)));
                }
                else
                {
                    var scopeNames =
                        _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
                    confirmedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.GrantedScopes != null && c.GrantedScopes.Any() &&
                            scopeNames.All(s => c.GrantedScopes.All(g => g.Name == s)));
                }
            }

            return confirmedConsent;
        }
    }
}
