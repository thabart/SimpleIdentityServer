using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;

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

        public static Uri AddParametersInQuery(this Uri uri, RouteValueDictionary dic)
        {
            var uriBuilder = new UriBuilder(uri); 
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var keyPair in dic)
            {
                query[keyPair.Key] = keyPair.Value.ToString();
            }

            uriBuilder.Query = query.ToString();
            return new Uri(uriBuilder.ToString());
        }

        public static Uri AddParametersInFragment(this Uri uri, RouteValueDictionary dic)
        {
            var uriBuilder = new UriBuilder(uri); 
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var keyPair in dic)
            {
                query[keyPair.Key] = keyPair.Value.ToString();
            }

            uriBuilder.Fragment = query.ToString();
            return new Uri(uriBuilder.ToString());
        }
    }
}