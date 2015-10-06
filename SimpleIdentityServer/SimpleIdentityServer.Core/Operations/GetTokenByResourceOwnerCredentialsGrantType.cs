using System;
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

        public GetTokenByResourceOwnerCredentialsGrantType(
            IDataSource dataSource,
            ISecurityHelper securityHelper,
            ITokenHelper tokenHelper)
        {
            _dataSource = dataSource;
            _securityHelper = securityHelper;
            _tokenHelper = tokenHelper;
        }

        public GrantedToken Execute(
            string userName, 
            string password, 
            string clientId,
            string scope)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.MissingParameter, "username"));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.MissingParameter, "password"));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.MissingParameter, "client_id"));
            }

            var hashPassword = _securityHelper.ComputeHash(password);
            var resourceOwners = _dataSource.ResourceOwners;
            var clients = _dataSource.Clients;
            var resourceOwner = resourceOwners.FirstOrDefault(r => r.Id == userName && r.Password == hashPassword);
            var client = clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client == null)
            {
                throw new IdentityServerException("invalid_client", string.Format(ErrorDescriptions.ClientIsNotValid, "client_id"));
            }

            if (resourceOwner == null)
            {
                throw new IdentityServerException("invalid_grant", ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            var generatedToken = _tokenHelper.GenerateToken(scope);
            _dataSource.GrantedTokens.Add(generatedToken);
            _dataSource.SaveChanges();

            return generatedToken;
        }
    }
}