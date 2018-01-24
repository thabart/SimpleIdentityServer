using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public class AssetAggregate
    {
        public AssetAggregate()
        {
            Children = new List<AssetAggregate>();
        }

        public string Hash { get; set; }
        public string ResourceParentHash { get; set; }
        public string Name { get; set; }
        public string AuthorizationPolicyId { get; set; }
        public string Path { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDefaultWorkingDirectory { get; set; }
        public bool IsLocked { get; set; }
        public bool CanWrite { get; set; }
        public bool CanRead { get; set; }
        public IEnumerable<AssetAggregate> Children { get; set; }
    }
}
