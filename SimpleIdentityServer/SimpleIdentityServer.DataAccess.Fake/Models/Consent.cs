using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public class Consent
    {
        public string Id { get; set; }

        public Client Client { get; set; }

        public ResourceOwner ResourceOwner { get; set; }

        public List<Scope> GrantedScopes { get; set; }
    }
}
