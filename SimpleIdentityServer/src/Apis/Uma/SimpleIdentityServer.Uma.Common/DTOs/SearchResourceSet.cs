using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Uma.Common.DTOs
{
    [DataContract]
    public class SearchResourceSet
    {
        [DataMember(Name = SearchResourceSetNames.Ids)]
        public IEnumerable<string> Ids { get; set; }
        [DataMember(Name = SearchResourceSetNames.Names)]
        public IEnumerable<string> Names { get; set; }
        [DataMember(Name = SearchResourceSetNames.Types)]
        public IEnumerable<string> Types { get; set; }
        [DataMember(Name = SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }
    }
}
