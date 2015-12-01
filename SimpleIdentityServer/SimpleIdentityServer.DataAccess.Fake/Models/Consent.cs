using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public class Consent
    {
        public string Id { get; set; }

        public Client Client { get; set; }

        public ResourceOwner ResourceOwner { get; set; }

        public List<Scope> GrantedScopes { get; set; }

        public List<string> Claims { get; set; }
    }
}
