using System;

namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class IdProvider
    {
        public string OpenIdWellKnownUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
