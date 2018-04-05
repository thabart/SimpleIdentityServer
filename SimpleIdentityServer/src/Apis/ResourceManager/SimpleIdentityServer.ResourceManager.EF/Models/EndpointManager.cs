namespace SimpleIdentityServer.ResourceManager.EF.Models
{
    public class EndpointManager
    {
        public string ManagerUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SourceUrl { get; set; }
        public string AuthUrl { get; set; }
        public virtual Endpoint SourceEndpoint { get; set; }
    }
}
