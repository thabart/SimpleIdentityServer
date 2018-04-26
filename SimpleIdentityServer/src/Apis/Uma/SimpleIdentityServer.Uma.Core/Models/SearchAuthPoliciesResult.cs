using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Models
{
    public class SearchAuthPoliciesResult
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public IEnumerable<Policy> Content { get; set; }
    }
}
