using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Uma.Common.DTOs
{
    [DataContract]
    public class SearchResourceSetResponse
    {
        [JsonProperty(SearchResponseNames.Content)]
        [DataMember(Name = SearchResponseNames.Content)]
        public IEnumerable<ResourceSetResponse> Content { get; set; }

        [JsonProperty(SearchResponseNames.TotalResults)]
        [DataMember(Name = SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [JsonProperty(SearchResponseNames.StartIndex)]
        [DataMember(Name = SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
