using System;

namespace WebApiContrib.Core.Storage
{
    public class DatedRecord<T>
    {
        public DateTime CreateDate { get; set; }

        public T Obj { get; set; }
    }
}
