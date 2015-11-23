using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    [DataContract]
    public class JsonWebKeySet
    {
        /// <summary>
        /// Gets or sets the array of JWK values.
        /// </summary>
        [DataMember(Name = "keys")]
        public List<Dictionary<string, object>> Keys { get; set; }
    }
}
