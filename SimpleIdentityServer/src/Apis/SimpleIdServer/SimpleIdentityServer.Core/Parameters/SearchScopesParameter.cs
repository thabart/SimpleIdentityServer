using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Parameters
{
    public class SearchScopesParameter
    {
        public SearchScopesParameter()
        {
            ScopeNames = new List<string>();
            IsPagingEnabled = true;
            StartIndex = 0;
            Count = 500;
        }

        public IEnumerable<string> ScopeNames { get; set; }
        public bool IsPagingEnabled { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}
