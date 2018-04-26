using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    [DataContract]
    public class SearchClientsRequest
    {
        [JsonProperty(Constants.SearchClientNames.ClientNames)]
        [DataMember(Name = Constants.SearchClientNames.ClientNames)]
        public IEnumerable<string> ClientNames { get; set; }

        [JsonProperty(Constants.SearchClientNames.ClientIds)]
        [DataMember(Name = Constants.SearchClientNames.ClientIds)]
        public IEnumerable<string> ClientIds { get; set; }

        [JsonProperty(Constants.SearchClientNames.ClientTypes)]
        [DataMember(Name = Constants.SearchClientNames.ClientTypes)]
        public IEnumerable<int> ClientTypes { get; set; }

        [JsonProperty(Constants.SearchResponseNames.StartIndex)]
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [JsonProperty(Constants.SearchResponseNames.TotalResults)]
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
    }
}
