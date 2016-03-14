﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNet.Routing;
using System;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class UriExtensions
    {
        public static Uri AddParameter(this Uri uri, string parameterName, string parameterValue)
        {
            var uriBuilder = new UriBuilder(uri);
            var query = Microsoft.AspNet.WebUtilities.QueryHelpers.ParseQuery(uriBuilder.Query);
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
            var query = Microsoft.AspNet.WebUtilities.QueryHelpers.ParseQuery(uriBuilder.Query);
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
            var query = Microsoft.AspNet.WebUtilities.QueryHelpers.ParseQuery(uriBuilder.Query);
            foreach (var keyPair in dic)
            {
                query[keyPair.Key] = keyPair.Value.ToString();
            }

            uriBuilder.Fragment = query.ToString();
            return new Uri(uriBuilder.ToString());
        }
    }
}