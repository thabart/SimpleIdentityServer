namespace SimpleIdentityServer.Client.DTOs.Request
{
    public class TokenRequest
    {
        public GrantTypeRequest grant_type { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string scope { get; set; }

        public string client_id { get; set; }
    }
}
