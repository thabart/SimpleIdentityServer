using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake
{
    public class FakeDataSource
    {
        private static FakeDataSource _instance = null;

        private readonly List<Client> _clients;

        private readonly List<RedirectionUrl> _redirectionUrls;

        private readonly List<ResourceOwner> _resourceOwners;

        private List<Scope> _scopes;

        private FakeDataSource()
        {
            _clients = new List<Client>();
            _redirectionUrls = new List<RedirectionUrl>();
            _resourceOwners = new List<ResourceOwner>();
            _scopes = new List<Scope>();
        }

        public static FakeDataSource Instance()
        {
            if (_instance == null)
            {
                _instance = new FakeDataSource();
            }

            return _instance;
        }

        public List<Client> Clients
        {
            get
            {
                return _clients;
            }
        }

        public List<RedirectionUrl> RedirectionUrls
        {
            get
            {
                return _redirectionUrls;
            }
        }

        public List<ResourceOwner> ResourceOwners
        {
            get
            {
                return _resourceOwners;
            }
        }

        public List<Scope> Scopes
        {
            get
            {
                return _scopes;
            }
        }
    }
}
