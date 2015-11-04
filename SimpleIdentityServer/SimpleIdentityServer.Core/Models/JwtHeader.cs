namespace SimpleIdentityServer.Core.Models
{
    public class JwtHeader
    {
        public string typ { get; set; }

        public string cty { get; set; }

        public string alg { get; set; }
    }
}
