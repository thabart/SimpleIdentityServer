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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IScopeValidator
    {
        ScopeValidationResult Check(string scope, Core.Common.Models.Client client);
    }

    public class ScopeValidationResult
    {
        public ScopeValidationResult(bool isValid)
        {
            IsValid = isValid;
        }

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public ICollection<string> Scopes { get; set; }
    }

    internal class ScopeValidator : IScopeValidator
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        public ScopeValidator(IParameterParserHelper parameterParserHelper)
        {
            _parameterParserHelper = parameterParserHelper;
        }

        public ScopeValidationResult Check(string scope, Core.Common.Models.Client client)
        {
            var emptyList = new List<string>();
            var scopes = _parameterParserHelper.ParseScopes(scope);
            if (!scopes.Any())
            {
                return new ScopeValidationResult(false)
                {
                    ErrorMessage = string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, scope)
                };
            }

            var duplicates = scopes.GroupBy(p => p)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicates.Any())
            {
                return new ScopeValidationResult(false)
                {
                    ErrorMessage = string.Format(ErrorDescriptions.DuplicateScopeValues, string.Join(",", duplicates))
                };
            }

            var scopeAllowed = client.AllowedScopes.Select(a => a.Name).ToList();
            var scopesNotAllowedOrInvalid = scopes
                .Where(s => !scopeAllowed.Contains(s))
                .ToList();
            if (scopesNotAllowedOrInvalid.Any())
            {
                return new ScopeValidationResult(false)
                {
                    ErrorMessage = string.Format(ErrorDescriptions.ScopesAreNotAllowedOrInvalid, string.Join(",", scopesNotAllowedOrInvalid))
                };
            }

            return new ScopeValidationResult(true)
            {
                Scopes = scopes
            };
        }
    }
}
