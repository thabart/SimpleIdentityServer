using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Api.DTOs.Response
{
    [DataContract]
    public class JsonWebKeySet
    {
        [DataMember(Name = "keys")]
        public IList<Dictionary<string, string>> JsonWebKeys { get; set; }
    }
}