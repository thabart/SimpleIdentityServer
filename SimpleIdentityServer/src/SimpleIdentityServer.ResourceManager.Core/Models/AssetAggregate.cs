using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public class AssetAggregate
    {
        public string Hash { get; set; }
        public string ResourceParentHash { get; set; }
        public string Name { get; set; }
        public string AuthorizationPolicyId { get; set; }
        public string Path { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDefaultWorkingDirectory { get; set; }
        public IEnumerable<AssetAggregate> Children { get; set; }
    }
}
