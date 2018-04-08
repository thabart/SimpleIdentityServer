using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    public class SearchClientsRequest
    {
        [DataMember(Name = Constants.SearchClientNames.ClientNames)]
        public IEnumerable<string> ClientNames { get; set; }
        
        [DataMember(Name = Constants.SearchClientNames.ClientIds)]
        public IEnumerable<string> ClientIds { get; set; }

        [DataMember(Name = Constants.SearchClientsResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [DataMember(Name = Constants.SearchClientsResponseNames.TotalResults)]
        public int NbResults { get; set; }
    }
}
