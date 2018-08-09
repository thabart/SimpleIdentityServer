namespace SimpleBus.InMemory
{
    public class InMemoryOptions
    {
        public InMemoryOptions()
        {
            Url = "http://localhost:61000/busHub";
            IsRetryEnabled = true;
            RetryTimestampInMs = 3000;
        }

        public string Url { get; set; }
        public string ServerName { get; set; }
        public bool IsRetryEnabled { get; set; }
        public int RetryTimestampInMs { get; set; }
    }
}
