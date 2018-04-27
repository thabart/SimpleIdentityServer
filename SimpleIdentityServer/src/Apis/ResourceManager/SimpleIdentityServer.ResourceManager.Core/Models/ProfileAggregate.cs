namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public class ProfileAggregate
    {
        public string Subject { get; set; }
        public string OpenidUrl { get; set; }
        public string AuthUrl { get; set; }
        public string ScimUrl { get; set; }
    }
}
