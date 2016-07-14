using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace WsFederation.Extensions
{
    public static class DictionaryExtensionscs
    {
        #region Public static methods

        public static string Get(this Dictionary<string, StringValues> collection, string name)
        {
            var str = string.Empty;
            var sv = default(StringValues);
            if (collection.TryGetValue("wa", out sv))
            {
                str = sv.First();
            }

            return str;
        }

        #endregion
    }
}
