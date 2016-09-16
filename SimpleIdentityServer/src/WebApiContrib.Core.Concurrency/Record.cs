using System;

namespace WebApiContrib.Core.Concurrency
{
    public class Record
    {
        public ConcurrentObject Obj { get; set; }

        public string Key { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }
    }
}
