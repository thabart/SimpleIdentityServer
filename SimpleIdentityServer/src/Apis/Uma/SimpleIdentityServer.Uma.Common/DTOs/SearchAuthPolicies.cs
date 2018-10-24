using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Uma.Common.DTOs
{
    [DataContract]
    public class SearchAuthPolicies
    {
        [JsonProperty(SearchAuthPolicyNames.Ids)]
        [DataMember(Name = SearchAuthPolicyNames.Ids)]
        public IEnumerable<string> Ids { get; set; }

        [JsonProperty(SearchAuthPolicyNames.ResourceIds)]
        [DataMember(Name = SearchAuthPolicyNames.ResourceIds)]
        public IEnumerable<string> ResourceIds { get; set; }

        [JsonProperty(SearchResponseNames.StartIndex)]
        [DataMember(Name = SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [JsonProperty(SearchResponseNames.TotalResults)]
        [DataMember(Name = SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }
    }
}
