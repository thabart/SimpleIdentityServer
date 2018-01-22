using System;

namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public class ResourceAggregate
    {
        public string ResourceId { get; set; }
        public string Name { get; set; }
        public string AuthorizationPolicyId { get; set; }
        public string ResourceParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
