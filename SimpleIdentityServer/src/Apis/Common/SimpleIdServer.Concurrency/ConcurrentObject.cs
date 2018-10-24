using System;

namespace SimpleIdServer.Concurrency
{
    public class ConcurrentObject
    {
        public string Etag { get; set; }
        public DateTime DateTime { get; set; }
    }
}
