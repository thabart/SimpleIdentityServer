namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public class GrantedToken
    {
        public string AccessToken { get; set; }

        public string IdToken { get; set; }

        public string TokenType { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string Scope { get; set; }
    }
}
