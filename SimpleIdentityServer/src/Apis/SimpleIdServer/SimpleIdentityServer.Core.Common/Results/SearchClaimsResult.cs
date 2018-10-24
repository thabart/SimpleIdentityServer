using SimpleIdentityServer.Core.Common.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Common.Results
{
    public class SearchClaimsResult
    {
        public IEnumerable<ClaimAggregate> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
