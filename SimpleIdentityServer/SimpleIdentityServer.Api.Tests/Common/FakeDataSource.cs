using System.Data.Entity;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.DataAccess.Models;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class FakeDataSource : IDataSource
    {
        private FakeDbSet<GrantedToken> _grantedTokens;

        private FakeDbSet<ResourceOwner> _resourceOwners;

        private FakeDbSet<Client> _clients;

        private FakeDbSet<Scope> _scopes;

        public FakeDataSource()
        {
            _grantedTokens = new FakeDbSet<GrantedToken>();
            _resourceOwners = new FakeDbSet<ResourceOwner>();
            _clients = new FakeDbSet<Client>();
            _scopes = new FakeDbSet<Scope>();
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

        public IDbSet<Scope> Scopes
        {
            get
            {
                return _scopes;
            }
            set
            {
                _scopes = (FakeDbSet<Scope>)value;
            }
        }

        public void SaveChanges()
        {
        }
    }
}