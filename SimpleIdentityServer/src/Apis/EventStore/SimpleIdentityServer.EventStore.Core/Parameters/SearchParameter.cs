namespace SimpleIdentityServer.EventStore.Core.Parameters
{
    public class SearchParameter
    {
        public SearchParameter()
        {
            Count = 100;
            StartIndex = 0;
        }

        public int StartIndex { get; set; }
        public int Count { get; set; }
        public string Filter { get; set; }
    }
}
