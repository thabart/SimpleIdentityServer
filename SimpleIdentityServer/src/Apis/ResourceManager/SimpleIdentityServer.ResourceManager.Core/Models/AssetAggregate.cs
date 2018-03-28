using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public class AssetAggregateAuthPolicy
    {
        public string AuthPolicyId { get; set; }
    }

    public class AssetAggregate
    {
        public AssetAggregate()
        {
            Children = new List<AssetAggregate>();
            AuthorizationPolicies = new List<AssetAggregateAuthPolicy>();
        }

        public string Hash { get; set; }
        public string ResourceParentHash { get; set; }
        public string ResourceId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string MimeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDefaultWorkingDirectory { get; set; }
        public bool IsLocked { get; set; }
        public bool CanWrite { get; set; }
        public bool CanRead { get; set; }
        public ICollection<AssetAggregate> Children { get; set; }
        public ICollection<AssetAggregateAuthPolicy> AuthorizationPolicies { get; set; }
    }
}
