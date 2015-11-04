using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IScopeValidator
    {
        List<string> ValidateAllowedScopes(string scope, Client client);
    }

    public class ScopeValidator : IScopeValidator
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        public ScopeValidator(IParameterParserHelper parameterParserHelper)
        {
            _parameterParserHelper = parameterParserHelper;
        }

        public List<string> ValidateAllowedScopes(string scope, Client client)
        {
            var result = new List<string>();
            if (!ValidateScope(scope))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.ParameterIsNotCorrect, "scope"));
            }

            var scopes = _parameterParserHelper.ParseScopeParameters(scope);
            if (scopes.Any())
            {
                var duplicates = scopes.GroupBy(p => p)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);
                if (duplicates.Any())
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.DuplicateScopeValues, string.Join(",", duplicates)));
                }

                var scopeAllowed = client.AllowedScopes.Select(a => a.Name).ToList();
                var scopesNotAllowedOrInvalid = scopes.Where(s => !scopeAllowed.Contains(s));
                if (scopesNotAllowedOrInvalid.Count() > 0)
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidScope,
                        string.Format(ErrorDescriptions.ScopesAreNotAllowedOrInvalid, string.Join(",", scopesNotAllowedOrInvalid)));
                }

                result = scopes;
            }

            return result;
        }
        
        private static bool ValidateScope(string scope)
        {
            var pattern = @"^\w+( +\w+)*$";
            var regularExpression = new Regex(pattern, RegexOptions.IgnoreCase);
            return regularExpression.IsMatch(scope);
        }
    }
}
