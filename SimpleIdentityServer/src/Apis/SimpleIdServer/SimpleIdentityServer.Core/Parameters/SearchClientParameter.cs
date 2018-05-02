using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Parameters
{
    public class SearchClientParameter
    {
        public SearchClientParameter()
        {
            ClientIds = new List<string>();
            ClientNames = new List<string>();
            IsPagingEnabled = true;
            StartIndex = 0;
            Count = 500;
        }

        public OrderParameter Order { get; set; }
        public IEnumerable<string> ClientIds { get; set; }
        public IEnumerable<string> ClientNames { get; set; }
        public IEnumerable<int> ClientTypes { get; set; }

        public bool IsPagingEnabled { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}
