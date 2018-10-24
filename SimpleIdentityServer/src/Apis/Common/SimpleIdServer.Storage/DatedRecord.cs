using System;

namespace SimpleIdServer.Storage
{
    public class DatedRecord<T>
    {
        public DateTime CreateDate { get; set; }
        public T Obj { get; set; }
    }
}
