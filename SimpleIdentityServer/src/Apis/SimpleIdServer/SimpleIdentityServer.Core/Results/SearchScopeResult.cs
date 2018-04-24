using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Results
{
    public class SearchScopeResult
    {
        public IEnumerable<Scope> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
