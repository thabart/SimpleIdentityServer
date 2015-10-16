namespace SimpleIdentityServerClient.DTOs.Response
{
    public class GrantedToken
    {
        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public int ExpiredIn { get; set; }

        public string RefreshToken { get; set; }

        public string Scope { get; set; }
    }
}
