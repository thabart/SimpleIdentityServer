using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    [DataContract]
    public class JweProtectedHeader
    {
        [DataMember(Name = "alg")]
        public string Alg { get; set; }


        [DataMember(Name = "enc")]
        public string Enc { get; set; }

        [DataMember(Name = "kid")]
        public string Kid { get; set; }
    }
}
