using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    public class SearchClientsResponse
    {
        [JsonProperty(Constants.SearchClientsResponseNames.Content)]
        [DataMember(Name = Constants.SearchClientsResponseNames.Content)]
        public IEnumerable<ClientResponse> Content { get; set; }

        [JsonProperty(Constants.SearchClientsResponseNames.TotalResults)]
        [DataMember(Name = Constants.SearchClientsResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [JsonProperty(Constants.SearchClientsResponseNames.StartIndex)]
        [DataMember(Name = Constants.SearchClientsResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
