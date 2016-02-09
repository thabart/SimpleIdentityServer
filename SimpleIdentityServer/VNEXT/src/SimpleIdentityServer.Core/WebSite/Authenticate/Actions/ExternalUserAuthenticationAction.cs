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
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface IExternalUserAuthenticationAction
    {
        List<Claim> Execute(List<Claim> claims, string providerType);
    }

    public sealed class ExternalUserAuthenticationAction : IExternalUserAuthenticationAction
    {
        private class MappingRule
        {
            public string Type { get; set; }

            public string OpenIdType { get; set; }
        }

        private readonly Dictionary<string, List<MappingRule>> _mappingRulesToOpenIdClaims;

        #region Constructor

        public ExternalUserAuthenticationAction()
        {
            _mappingRulesToOpenIdClaims = new Dictionary<string, List<MappingRule>>
            {
                {
                    Constants.ProviderTypeNames.Microsoft,
                    new List<MappingRule>
                    {
                        new MappingRule
                        {
                            Type = Constants.MicrosoftClaimNames.Id,
                            OpenIdType = Jwt.Constants.StandardResourceOwnerClaimNames.Subject
                        },
                        new MappingRule
                        {
                            Type = Constants.MicrosoftClaimNames.Name,
                            OpenIdType = Jwt.Constants.StandardResourceOwnerClaimNames.Name
                        },
                        new MappingRule
                        {
                            Type = Constants.MicrosoftClaimNames.FirstName,
                            OpenIdType = Jwt.Constants.StandardResourceOwnerClaimNames.GivenName
                        },
                        new MappingRule
                        {
                            Type = Constants.MicrosoftClaimNames.LastName,
                            OpenIdType = Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName
                        }
                    }
                }
            };
        }

        #endregion

        #region Public methods

        public List<Claim> Execute(
            List<Claim> claims,
            string providerType)
        {
            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException("claims");
            }

            if (string.IsNullOrWhiteSpace(providerType))
            {
                throw new ArgumentNullException("providerType");
            }

            if (!Constants.SupportedProviderTypes.Contains(providerType))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    string.Format(ErrorDescriptions.TheExternalProviderIsNotSupported, providerType));
            }

            var result = new List<Claim>();
            switch (providerType)
            {
                case Constants.ProviderTypeNames.Microsoft:
                    result = ConvertMicrosoftClaims(claims);
                    break;
            }

            return result;
        }

        #endregion

        #region Private methods

        private List<Claim> ConvertMicrosoftClaims(List<Claim> claims)
        {
            var result = new List<Claim>();
            var mappingRules = _mappingRulesToOpenIdClaims[Constants.ProviderTypeNames.Microsoft];
            claims.ForEach(c =>
            {
                var mappingRule = mappingRules.FirstOrDefault(m => m.Type == c.Type);
                if (mappingRule != null)
                {
                    result.Add(new Claim(mappingRule.OpenIdType, c.Value));
                }
            });

            return result;
        }

        #endregion
    }
}
