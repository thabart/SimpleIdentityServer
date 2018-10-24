using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Models
{
    public class SearchResourceSetResult
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public IEnumerable<ResourceSet> Content { get; set; }
    }
}
