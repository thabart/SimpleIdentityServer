using System;
using System.DirectoryServices.Protocols;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Extensions
{
    internal static class DirectoryAttributeExtensions
    {
        public static string TryGetAttributeValueAsString(this DirectoryAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(attr));
            }

            string value = null;
            if (attr != null && attr.Count > 0)
            {
                value = attr[0] as string;
            }

            return value;
        }
    }
}
