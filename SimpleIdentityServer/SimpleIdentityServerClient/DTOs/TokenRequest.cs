namespace SimpleIdentityServerClient.DTOs
{
    public class TokenRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Scope { get; set; }

        public string ClientId { get; set; }
    }
}
