namespace SimpleIdentityServer.Scim.Mapping.Ad.Models
{
    public class AdConfiguration
    {
        public string IpAdr { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DistinguishedName { get; set; }
        public string UserFilter { get; set; }
        public bool IsEnabled { get; set; }
    }
}