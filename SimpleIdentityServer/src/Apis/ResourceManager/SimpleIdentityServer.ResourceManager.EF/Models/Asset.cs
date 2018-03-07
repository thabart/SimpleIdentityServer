using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class Asset
    {
        public string Hash { get; set; }
        public string ResourceParentHash { get; set; }
        public string ResourceId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string MimeType { get; set; }
        public bool IsDefaultWorkingDirectory { get; set; }
        public bool IsLocked { get; set; }
        public bool CanWrite { get; set; }
        public bool CanRead { get; set; }
        public DateTime CreateDateTime { get; set; }
        public virtual Asset Parent { get; set; }
        public virtual ICollection<Asset> Children { get; set; }
        public virtual ICollection<AssetAuthPolicy> AuthPolicies { get; set; }
    }
}
