using System;

namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class Endpoint
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public DateTime CreateDateTime { get; set; }
        public int Order { get; set; }
        public virtual EndpointManager Manager { get; set; }
    }
}
