using System.Data.Entity;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Helpers;
using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake
{
    public class FakeDataSource : IDataSource
    {
        private readonly ISecurityHelper _securityHelper;

        private FakeDbSet<GrantedToken> _grantedTokens;

        private FakeDbSet<ResourceOwner> _resourceOwners;

        private FakeDbSet<Client> _clients;

        private FakeDbSet<Scope> _scopes;

        public FakeDataSource(ISecurityHelper securityHelper)
        {
            _securityHelper = securityHelper;
            _grantedTokens = new FakeDbSet<GrantedToken>();
            _resourceOwners = new FakeDbSet<ResourceOwner>();
            _clients = new FakeDbSet<Client>();
            _scopes = new FakeDbSet<Scope>();

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

        private void Initialize()
        {
            var resourceOwner = new ResourceOwner
            {
                Id = "administrator",
                Password = _securityHelper.ComputeHash("administrator")
            };
            var scope = new Scope
            {
                Name = "firstScope"
            };
            var client = new Client
            {
                ClientId = "WebSite",
                AllowedScopes = new List<Scope>
                {
                    scope
                }
            };

            _resourceOwners.Add(resourceOwner);
            _clients.Add(client);
            _scopes.Add(scope);
        }
    }
}