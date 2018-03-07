using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.EventStore.EF.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> GetParameters(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Split('|');
        }
    }
}
