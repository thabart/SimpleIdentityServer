using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake
{
    public class FakeDataSource
    {
        private static FakeDataSource _instance = null;

        private List<Client> _clients;

        private List<RedirectionUrl> _redirectionUrls;

        private List<ResourceOwner> _resourceOwners;

        private List<Scope> _scopes;

        private List<Consent> _consents;

        private FakeDataSource()
        {
            Init();
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
            set { _clients = value; }
        }

        public List<RedirectionUrl> RedirectionUrls
        {
            get
            {
                return _redirectionUrls;
            }
            set { _redirectionUrls = value; }
        }

        public List<ResourceOwner> ResourceOwners
        {
            get
            {
                return _resourceOwners;
            }
            set { _resourceOwners = value; }
        }

        public List<Scope> Scopes
        {
            get
            {
                return _scopes;
            }
            set { _scopes = value; }
        }


        public List<Consent> Consents
        {
            get
            {
                return _consents;
            } set
            {
                _consents = value;
            }
        }

        public void Init()
        {
            _clients = new List<Client>();
            _redirectionUrls = new List<RedirectionUrl>();
            _resourceOwners = new List<ResourceOwner>();
            _scopes = new List<Scope>();
            _consents = new List<Consent>();
        }
    }
}
