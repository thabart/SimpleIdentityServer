namespace SimpleIdentityServer.Scim.Mapping.Ad.Models
{
    public class AdConfiguration
    {
        public string Path { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsEnabled { get; set; }
        public string UserFilter { get; set; }
    }
}