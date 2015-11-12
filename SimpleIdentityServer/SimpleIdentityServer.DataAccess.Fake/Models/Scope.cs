using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public enum ScopeType
    {
        ProtectedApi,
        ResourceOwner
    }

    public class Scope
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsInternal { get; set; }

        public bool IsExposed { get; set; }

        public bool IsDisplayedInConsent { get; set; }

        public ScopeType Type { get; set; }

        public List<string> Claims { get; set; }
    }
}
