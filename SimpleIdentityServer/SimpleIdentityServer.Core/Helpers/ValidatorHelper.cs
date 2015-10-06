using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IValidatorHelper
    {
        bool ValidateScope(string scope);
    }

    public class ValidatorHelper : IValidatorHelper
    {
        public bool ValidateScope(string scope)
        {
            var pattern = @"^\w+( +\w+)*$";
            var regularExpression = new Regex(pattern, RegexOptions.IgnoreCase);
            return regularExpression.IsMatch(scope);
        }
    }
}
