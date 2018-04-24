using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    [DataContract]
    public class SearchResourceOwnersRequest
    {
        [JsonProperty(Constants.SearchResourceOwnerNames.Subjects)]
        [DataMember(Name = Constants.SearchResourceOwnerNames.Subjects)]
        public IEnumerable<string> Subjects { get; set; }

        [JsonProperty(Constants.SearchResponseNames.StartIndex)]
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [JsonProperty(Constants.SearchResponseNames.TotalResults)]
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
    }
}
