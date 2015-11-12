using System.Runtime.Serialization;

namespace SimpleIdentityServer.Api.DTOs.Request
{
    public class TokenRequest
    {
        public GrantTypeRequest grant_type { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string scope { get; set; }

        public string client_id { get; set; }

        public string code { get; set; }

        public string redirect_uri { get; set; }
    }
}