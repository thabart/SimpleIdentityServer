using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    [DataContract]
    public class SearchScopesResponse
    {
        [JsonProperty(Constants.SearchResponseNames.Content)]
        [DataMember(Name = Constants.SearchResponseNames.Content)]
        public IEnumerable<ScopeResponse> Content { get; set; }

        [JsonProperty(Constants.SearchResponseNames.TotalResults)]
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [JsonProperty(Constants.SearchResponseNames.StartIndex)]
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
