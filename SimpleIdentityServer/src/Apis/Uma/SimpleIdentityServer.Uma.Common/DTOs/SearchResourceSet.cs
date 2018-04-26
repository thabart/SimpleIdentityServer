using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Uma.Common.DTOs
{
    [DataContract]
    public class SearchResourceSet
    {
        [JsonProperty(SearchResourceSetNames.Ids)]
        [DataMember(Name = SearchResourceSetNames.Ids)]
        public IEnumerable<string> Ids { get; set; }

        [JsonProperty(SearchResourceSetNames.Names)]
        [DataMember(Name = SearchResourceSetNames.Names)]
        public IEnumerable<string> Names { get; set; }

        [JsonProperty(SearchResourceSetNames.Types)]
        [DataMember(Name = SearchResourceSetNames.Types)]
        public IEnumerable<string> Types { get; set; }

        [JsonProperty(SearchResponseNames.StartIndex)]
        [DataMember(Name = SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [JsonProperty(SearchResponseNames.TotalResults)]
        [DataMember(Name = SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }
    }
}
