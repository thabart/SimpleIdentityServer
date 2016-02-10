using System;
using System.Web;
using System.Web.Routing;

namespace SimpleIdentityServer.Host.Extensions
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

        /// <summary>
        /// Add the given parameter in the query string.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add the given parameters in the fragment.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
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