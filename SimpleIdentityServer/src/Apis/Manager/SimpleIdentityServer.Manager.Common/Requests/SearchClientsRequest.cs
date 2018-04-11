using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    public class SearchClientsRequest
    {
        [JsonProperty(Constants.SearchClientNames.ClientNames)]
        [DataMember(Name = Constants.SearchClientNames.ClientNames)]
        public IEnumerable<string> ClientNames { get; set; }

        [JsonProperty(Constants.SearchClientNames.ClientIds)]
        [DataMember(Name = Constants.SearchClientNames.ClientIds)]
        public IEnumerable<string> ClientIds { get; set; }

        [JsonProperty(Constants.SearchClientsResponseNames.StartIndex)]
        [DataMember(Name = Constants.SearchClientsResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [JsonProperty(Constants.SearchClientsResponseNames.TotalResults)]
        [DataMember(Name = Constants.SearchClientsResponseNames.TotalResults)]
        public int NbResults { get; set; }
    }
}
