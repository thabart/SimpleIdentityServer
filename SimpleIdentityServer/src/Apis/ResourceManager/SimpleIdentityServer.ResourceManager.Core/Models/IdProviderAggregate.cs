using System;

namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public class IdProviderAggregate
    {
        public string OpenIdWellKnownUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
