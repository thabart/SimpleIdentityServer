namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class AssetAuthPolicy
    {
        public string AssetHash { get; set; }
        public string AuthPolicyId { get; set; }
        public virtual Asset Asset { get; set; }
    }
}
