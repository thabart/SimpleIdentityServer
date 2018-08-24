using SimpleIdentityServer.Scim.Common.Models;
using System;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    public interface IUserFilterParser
    {
        string Parse(string filter, Representation representation);
    }

    internal sealed class UserFilterParser : IUserFilterParser
    {
        public string Parse(string filter, Representation representation)
        {
            var regex = new Regex("\\${(\\w|\\.)*}", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(filter);
            foreach(var match in matches)
            {
                var fullPath = match.ToString()
                    .Replace("${", "")
                    .Replace("}", "");
                var attribute = GetAttribute(representation, fullPath);
                if (attribute == null)
                {
                    throw new InvalidOperationException($"the attribute {fullPath} doesn't exist");
                }

                filter = filter.Replace("${" + fullPath + "}", attribute.Value);
            }

            return filter;
        }

        private static RepresentationAttribute GetAttribute(Representation representation, string fullPath)
        {
            foreach (var attribute in representation.Attributes)
            {
                if(attribute.FullPath == fullPath)
                {
                    return attribute;
                }
            }

            return null;
        }
    }
}