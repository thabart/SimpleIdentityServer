using System.Collections.Generic;

namespace SimpleIdentityServer.EventStore.Core.Results
{
    public class SearchResult
    {
        public SearchResult(int totalResults, IEnumerable<dynamic> content)
        {
            TotalResults = totalResults;
            Content = content;
        }

        public int TotalResults { get; private set; }
        public IEnumerable<dynamic> Content { get; private set; }
    }
}
