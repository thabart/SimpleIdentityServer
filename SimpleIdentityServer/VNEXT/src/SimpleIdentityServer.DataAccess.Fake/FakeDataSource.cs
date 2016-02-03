using SimpleIdentityServer.DataAccess.Fake.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake
{
    public class FakeDataSource
    {
        public List<Client> Clients { get; set;}

        public List<RedirectionUrl> RedirectionUrls { get; set;}
        
        public List<ResourceOwner> ResourceOwners { get; set;}

        public List<Scope> Scopes { get; set;}


        public List<Consent> Consents { get; set;}

        public List<AuthorizationCode> AuthorizationCodes { get; set;}

        public List<Translation> Translations { get; set;}

        public List<JsonWebKey> JsonWebKeys { get; set;}

        public List<GrantedToken> GrantedTokens { get; set;}

        public void Init()
        {
            Clients = new List<Client>();
            RedirectionUrls = new List<RedirectionUrl>();
            ResourceOwners = new List<ResourceOwner>();
            Scopes = new List<Scope>();
            Consents = new List<Consent>();
            AuthorizationCodes = new List<AuthorizationCode>();
            JsonWebKeys = new List<JsonWebKey>();
            GrantedTokens = new List<GrantedToken>();
            Translations = new List<Translation>();
        }
    }
}
