using System;
using System.Web;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class UriExtensions
    {
        public static Uri AddParameter(this Uri uri, string parameterName, string parameterValue)
        {
            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[parameterName] = parameterValue;
            uriBuilder.Query = query.ToString();
            return new Uri(uriBuilder.ToString());
        }
    }
}