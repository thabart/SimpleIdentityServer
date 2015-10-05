using System;
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

        private FakeDbSet<Client> _clients;

        public FakeDataSource(ISecurityHelper securityHelper)
        {
            _securityHelper = securityHelper;
            _grantedTokens = new FakeDbSet<GrantedToken>();
            _resourceOwners = new FakeDbSet<ResourceOwner>();
            _clients = new FakeDbSet<Client>();

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

        public IDbSet<Client> Clients
        {
            get
            {
                return _clients;
            }

            set
            {
                _clients = (FakeDbSet<Client>)value;
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
            var client = new Client
            {
                ClientId = "WebSite"
            };

            _resourceOwners.Add(resourceOwner);
            _clients.Add(client);
        }
    }
}