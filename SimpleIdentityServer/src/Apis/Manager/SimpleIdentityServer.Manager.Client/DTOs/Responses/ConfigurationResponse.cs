namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class ConfigurationResponse
    {
        public string JwsEndpoint { get; set; }
        public string JweEndpoint { get; set; }
        public string ClientsEndpoint { get; set; }
        public string ScopesEndpoint { get; set; }
        public string ResourceOwnersEndpoint { get; set; }
        public string ManageEndpoint { get; set; }
    }
}
