using System.Linq;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.Operations
{
    public interface IGetTokenByResourceOwnerCredentialsGrantType
    {
        GrantedToken Execute(
            string userName, 
            string password, 
            string clientId,
            string scope);
    }

    public class GetTokenByResourceOwnerCredentialsGrantType : IGetTokenByResourceOwnerCredentialsGrantType
    {
        private readonly IDataSource _dataSource;

        private readonly ISecurityHelper _securityHelper;

        private readonly ITokenHelper _tokenHelper;

        private readonly IValidatorHelper _validatorHelper;

        public GetTokenByResourceOwnerCredentialsGrantType(
            IDataSource dataSource,
            ISecurityHelper securityHelper,
            ITokenHelper tokenHelper,
            IValidatorHelper validatorHelper)
        {
            _dataSource = dataSource;
            _securityHelper = securityHelper;
            _tokenHelper = tokenHelper;
            _validatorHelper = validatorHelper;
        }

        public GrantedToken Execute(
            string userName, 
            string password, 
            string clientId,
            string scope)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode, 
                    string.Format(ErrorDescriptions.MissingParameter, "username"));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode, 
                    string.Format(ErrorDescriptions.MissingParameter, "password"));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode, 
                    string.Format(ErrorDescriptions.MissingParameter, "client_id"));
            }

            var hashPassword = _securityHelper.ComputeHash(password);
            var resourceOwners = _dataSource.ResourceOwners;
            var clients = _dataSource.Clients;
            var resourceOwner = resourceOwners.FirstOrDefault(r => r.Id == userName && r.Password == hashPassword);
            var client = clients.FirstOrDefault(c => c.ClientId == clientId);
            var allowedTokenScopes = string.Empty;
            if (client == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.ClientIsNotValid, "client_id"));
            }

            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            if (!string.IsNullOrWhiteSpace(scope))
            {
                if (!_validatorHelper.ValidateScope(scope))
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

                    allowedTokenScopes = string.Join(" ", scopes);
                }
            }

            var generatedToken = _tokenHelper.GenerateToken(allowedTokenScopes);
            _dataSource.GrantedTokens.Add(generatedToken);
            _dataSource.SaveChanges();

            return generatedToken;
        }
    }
}