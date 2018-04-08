using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    public class SearchClientsResponse
    {
        [DataMember(Name = Constants.SearchClientsResponseNames.Content)]
        public IEnumerable<ClientResponse> Content { get; set; }

        [DataMember(Name = Constants.SearchClientsResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [DataMember(Name = Constants.SearchClientsResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
