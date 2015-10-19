using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Errors;

using System.Linq;

using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IValidatorHelper
    {
        string ValidateAllowedScopes(string scope, Client client);

        Client ValidateExistingClient(string clientId);

        ResourceOwner ValidateResourceOwner(string userName, string password);

        RedirectionUrl ValidateAllowedRedirectionUrl(string url, Client client);
    }

    public class ValidatorHelper : IValidatorHelper
    {
        private readonly IDataSource _dataSource;

        private readonly ISecurityHelper _securityHelper;

        public ValidatorHelper(
            IDataSource dataSource,
            ISecurityHelper securityHelper)
        {
            _dataSource = dataSource;
            _securityHelper = securityHelper;
        }

        public string ValidateAllowedScopes(string scope, Client client)
        {
            var result = string.Empty;
            if (!ValidateScope(scope))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.ParameterIsNotCorrect, "scope"));
            }

            var scopes = scope.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
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

                result = string.Join(" ", scopes);
            }

            return result;
        }

        public Client ValidateExistingClient(string clientId)
        {
            var clients = _dataSource.Clients;
            var client = clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.ClientIsNotValid, "client_id"));
            }

            return client;
        }

        public ResourceOwner ValidateResourceOwner(string userName, string password)
        {
            var hashPassword = _securityHelper.ComputeHash(password);
            var resourceOwners = _dataSource.ResourceOwners;
            var resourceOwner = resourceOwners.FirstOrDefault(r => r.Id == userName
                && r.Password == hashPassword);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            return resourceOwner;
        }
        
        public RedirectionUrl ValidateAllowedRedirectionUrl(string url, Client client)
        {
            var redirectionUrl = client.RedirectionUrls.FirstOrDefault(r => r.Url == url);
            if (redirectionUrl == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectionUrl));
            }

            return redirectionUrl;
        }

        private bool ValidateScope(string scope)
        {
            var pattern = @"^\w+( +\w+)*$";
            var regularExpression = new Regex(pattern, RegexOptions.IgnoreCase);
            return regularExpression.IsMatch(scope);
        }
    }
}
