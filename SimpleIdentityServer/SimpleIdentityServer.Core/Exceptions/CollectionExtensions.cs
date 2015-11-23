using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Exceptions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T, TZ>(this Dictionary<T, TZ> firstDictionary,
            Dictionary<T, TZ> secondDictionary)
        {
            foreach (var keyPair in secondDictionary)
            {
                firstDictionary.Add(keyPair.Key, keyPair.Value);
            }
        }
    }
}
