using SimpleIdentityServer.Core.Common.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Results
{
    public class SearchClientResult
    {
        public IEnumerable<Client> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
