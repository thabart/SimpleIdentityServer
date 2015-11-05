namespace SimpleIdentityServer.Core.Jwt
{
    public class JwtHeader
    {
        public string typ { get; set; }

        public string cty { get; set; }

        public string alg { get; set; }
    }
}
