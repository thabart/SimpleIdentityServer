using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Parameters
{
    public class SearchAuthPoliciesParameter
    {
        public SearchAuthPoliciesParameter()
        {
            IsPagingEnabled = true;
        }

        public IEnumerable<string> Ids { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public bool IsPagingEnabled { get; set; }
    }
}
