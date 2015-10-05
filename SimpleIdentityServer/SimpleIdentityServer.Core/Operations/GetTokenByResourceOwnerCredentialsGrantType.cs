using System.Linq;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.DataAccess.Models;

namespace SimpleIdentityServer.Core.Operations
{
    public interface IGetTokenByResourceOwnerCredentialsGrantType
    {
        GrantedToken Execute(string userName, string password, string scope);
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

        public GrantedToken Execute(string userName, string password, string scope)
        {
            var hashPassword = _securityHelper.ComputeHash(password);
            var resourceOwners = _dataSource.ResourceOwners;
            var resourceOwner = resourceOwners.FirstOrDefault(r => r.Id == userName && r.Password == hashPassword);
            if (resourceOwner == null)
            {
                // TODO : throw the exception
            }

            var generatedToken = _tokenHelper.GenerateToken(scope);
            _dataSource.GrantedTokens.Add(generatedToken);
            _dataSource.SaveChanges();

            return generatedToken;
        }
    }
}