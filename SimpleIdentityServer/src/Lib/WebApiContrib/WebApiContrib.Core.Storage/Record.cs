using System;

namespace WebApiContrib.Core.Storage
{
    public class Record
    {
        public object Obj { get; set; }

        public string Key { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }
    }
}
