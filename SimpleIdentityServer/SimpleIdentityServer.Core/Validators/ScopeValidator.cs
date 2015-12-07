using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IScopeValidator
    {
        List<string> IsScopesValid(
            string scope,
            Client client,
            out string messageDescription);
    }

    public class ScopeValidator : IScopeValidator
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        public ScopeValidator(IParameterParserHelper parameterParserHelper)
        {
            _parameterParserHelper = parameterParserHelper;
        }

        public List<string> IsScopesValid(
            string scope, 
            Client client,
            out string messageDescription)
        {
            var emptyList = new List<string>();
            var result = new List<string>();
            messageDescription = string.Empty;
            if (!ValidateScope(scope))
            {
                messageDescription = string.Format(ErrorDescriptions.ParameterIsNotCorrect, "scope");
                return emptyList;
            }

            var scopes = _parameterParserHelper.ParseScopeParameters(scope);
            if (scopes.Any())
            {
                var duplicates = scopes.GroupBy(p => p)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicates.Any())
                {
                    messageDescription = string.Format(ErrorDescriptions.DuplicateScopeValues,
                        string.Join(",", duplicates));
                    return emptyList;
                }

                var scopeAllowed = client.AllowedScopes.Select(a => a.Name).ToList();
                var scopesNotAllowedOrInvalid = scopes
                    .Where(s => !scopeAllowed.Contains(s))
                    .ToList();
                if (scopesNotAllowedOrInvalid.Any())
                {
                    messageDescription = string.Format(ErrorDescriptions.ScopesAreNotAllowedOrInvalid,
                        string.Join(",", scopesNotAllowedOrInvalid));
                    return emptyList;
                }

                result = scopes;
            }

            return result;
        }
        
        private static bool ValidateScope(string scope)
        {
            const string pattern = @"^\w+( +\w+)*$";
            var regularExpression = new Regex(pattern, RegexOptions.IgnoreCase);
            return regularExpression.IsMatch(scope);
        }
    }
}
