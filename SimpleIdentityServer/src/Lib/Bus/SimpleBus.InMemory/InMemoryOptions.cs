namespace SimpleBus.InMemory
{
    public class InMemoryOptions
    {
        public InMemoryOptions()
        {
            Url = "http://localhost:61000/busHub";
        }

        public string Url { get; set; }
        public string ServerName { get; set; }
    }
}
