using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Db.EF.Models
{
    public class Representation
    {
        public string Id { get; set; }
        public string ResourceType { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string Version { get; set; }
        public virtual List<RepresentationAttribute> Attributes { get; set; }
    }
}
