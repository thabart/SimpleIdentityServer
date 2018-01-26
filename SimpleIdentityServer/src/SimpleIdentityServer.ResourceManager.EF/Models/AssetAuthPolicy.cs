using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class AssetAuthPolicy
    {
        public string AssetHash { get; set; }
        public string AuthPolicyId { get; set; }
        public bool IsOwner { get; set; }
        public virtual Asset Asset { get; set; }
    }
}
