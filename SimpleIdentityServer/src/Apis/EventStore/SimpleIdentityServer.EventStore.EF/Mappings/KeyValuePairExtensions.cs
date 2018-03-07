using System.Collections.Generic;

namespace SimpleIdentityServer.EventStore.EF.Mappings
{
    public static class KeyValuePairExtensions
    {
        public static bool IsEmpty<T,G>(this KeyValuePair<T,G> kvp)
        {
            return kvp.Equals(default(KeyValuePair<T, G>)) || kvp.Value == null;
        }
    }
}
