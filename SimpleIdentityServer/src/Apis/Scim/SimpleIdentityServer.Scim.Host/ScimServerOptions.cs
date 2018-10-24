using SimpleIdentityServer.Scim.Core.EF.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Host
{
    public class ScimServerConfiguration
    {
        public List<Representation> Representations { get; set; }
        public List<Schema> Schemas { get; set; }
    }

    public class ScimServerOptions
    {
        public ScimServerConfiguration ServerConfiguration { get; set; }
    }
}
