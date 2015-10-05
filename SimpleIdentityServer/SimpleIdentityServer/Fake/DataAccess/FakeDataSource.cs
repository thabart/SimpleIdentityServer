using System.Data.Entity;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Helpers;

namespace SimpleIdentityServer.Api.Fake.DataAccess
{
    public class FakeDataSource : IDataSource
    {
        private readonly ISecurityHelper _securityHelper;

        private FakeDbSet<GrantedToken> _grantedTokens;

        private FakeDbSet<ResourceOwner> _resourceOwners;

        public FakeDataSource(ISecurityHelper securityHelper)
        {
            _securityHelper = securityHelper;
            _grantedTokens = new FakeDbSet<GrantedToken>();
            _resourceOwners = new FakeDbSet<ResourceOwner>();

            Initialize();
        }

        public IDbSet<GrantedToken> GrantedTokens
        {
            get
            {
                return _grantedTokens;
            }

            set
            {
                _grantedTokens = (FakeDbSet<GrantedToken>)value;
            }
        }

        public IDbSet<ResourceOwner> ResourceOwners
        {
            get
            {
                return _resourceOwners;
            }

            set
            {
                _resourceOwners = (FakeDbSet<ResourceOwner>)value;
            }
        }

        public void SaveChanges()
        {
        }

        private void Initialize()
        {
            var resourceOwner = new ResourceOwner
            {
                Id = "administrator",
                Password = _securityHelper.ComputeHash("administrator")
            };

            _resourceOwners.Add(resourceOwner);
        }
    }
}