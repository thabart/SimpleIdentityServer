using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Models
{
    public class AdMapping
    {
        public string AttributeId { get; set; }
        public string AdPropertyName { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
