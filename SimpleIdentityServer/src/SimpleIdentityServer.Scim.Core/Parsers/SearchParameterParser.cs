#region copyright
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

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Errors;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    internal interface ISearchParameterParser
    {
        /// <summary>
        /// Parse the query and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="query">Query parameters.</param>
        /// <returns>Search parameters.</returns>
        SearchParameter ParseQuery(IQueryCollection query);
        /// <summary>
        /// Parse the json and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="json">JSON that will be parsed.</param>
        /// <returns>Search parameters.</returns>
        SearchParameter ParseJson(JObject obj);
    }

    public enum SortOrders
    {
        Ascending,
        Descending
    }

    public class SearchParameter
    {
        public SearchParameter()
        {
            Count = 100;
            StartIndex = 1;
            SortOrder = SortOrders.Ascending;
        }

        /// <summary>
        /// Names of resource attributes to return in the response.
        /// </summary>
        public IEnumerable<Filter> Attributes { get; set; }
        /// <summary>
        /// Names of resource attributes to be removed from the default set of attributes to return.
        /// </summary>
        public IEnumerable<Filter> ExcludedAttributes { get; set; }
        /// <summary>
        /// Filter used to request a subset of resources.
        /// </summary>
        public Filter Filter { get; set; }
        /// <summary>
        /// Indicate whose value SHALL be used to order the returned responses.
        /// </summary>
        public Filter SortBy { get; set; }
        /// <summary>
        /// In which the "sortBy" parameter is applied.
        /// </summary>
        public SortOrders SortOrder { get; set; }
        /// <summary>
        /// The 1-based index of the first query result.
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// The desired maximum number of query results per page.
        /// </summary>
        public int Count { get; set; }
    }

    internal class SearchParameterParser : ISearchParameterParser
    {

        private static class SortOrderNames
        {
            public static string Ascending = "ascending";
            public static string Descending = "descending";
        }

        private readonly IFilterParser _filterParser;

        public SearchParameterParser(IFilterParser filterParser)
        {
            _filterParser = filterParser;
        }

        /// <summary>
        /// Parse the query and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="query">Query parameters.</param>
        /// <returns>Search parameters.</returns>
        public SearchParameter ParseQuery(IQueryCollection query)
        {
            var result = new SearchParameter();
            if (query == null)
            {
                return result;
            }

            foreach(var key in query.Keys)
            {
                TrySetEnum((r) => result.Attributes = r.Select(a => GetFilter(a)), key, Constants.SearchParameterNames.Attributes, query);
                TrySetEnum((r) => result.ExcludedAttributes = r.Select(a => GetFilter(a)), key, Constants.SearchParameterNames.ExcludedAttributes, query);
                TrySetStr((r) => result.Filter = GetFilter(r), key, Constants.SearchParameterNames.Filter, query);
                TrySetStr((r) => result.SortBy = GetFilter(r), key, Constants.SearchParameterNames.SortBy, query);
                TrySetStr((r) => result.SortOrder = GetSortOrder(r), key, Constants.SearchParameterNames.SortOrder, query);
                TrySetInt((r) => result.StartIndex = r <= 0 ? result.StartIndex : r, key, Constants.SearchParameterNames.StartIndex, query);
                TrySetInt((r) => result.Count = r <= 0 ? result.Count : r, key, Constants.SearchParameterNames.Count, query);
            }

            return result;
        }

        /// <summary>
        /// Parse the json and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="json">JSON that will be parsed.</param>
        /// <returns>Search parameters.</returns>
        public SearchParameter ParseJson(JObject json)
        {
            var result = new SearchParameter();
            if (json == null)
            {
                return result;
            }

            JArray jArr;
            JValue jVal;
            if (TryGetToken(json, Constants.SearchParameterNames.Attributes, out jArr))
            {
                result.Attributes = (jArr.Values<string>()).Select(a => GetFilter(a));
            }

            if (TryGetToken(json, Constants.SearchParameterNames.ExcludedAttributes, out jArr))
            {
                result.ExcludedAttributes = (jArr.Values<string>()).Select(a => GetFilter(a));
            }

            if (TryGetToken(json, Constants.SearchParameterNames.Filter, out jVal))
            {
                result.Filter = GetFilter(jVal.Value<string>());
            }

            if (TryGetToken(json, Constants.SearchParameterNames.SortBy, out jVal))
            {
                result.SortBy = GetFilter(jVal.Value<string>());
            }

            if (TryGetToken(json, Constants.SearchParameterNames.SortOrder, out jVal))
            {
                result.SortOrder = GetSortOrder(jVal.Value<string>());
            }

            if (TryGetToken(json, Constants.SearchParameterNames.StartIndex, out jVal))
            {
                var i = GetInt(jVal.Value<string>(), Constants.SearchParameterNames.StartIndex);
                result.StartIndex = i <= 0 ? result.StartIndex : i;
            }

            if (TryGetToken(json, Constants.SearchParameterNames.Count, out jVal))
            {
                var i = GetInt(jVal.Value<string>(), Constants.SearchParameterNames.Count);
                result.Count = i <= 0 ? result.Count : i;
            }

            return result;
        }

        private Filter GetFilter(string value)
        {
            var filter = _filterParser.Parse(value);
            if (filter == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, Constants.SearchParameterNames.Filter));
            }

            return filter;
        }

        private static void TrySetEnum(Action<IEnumerable<string>> setParameterCallback, string key, string value, IQueryCollection query)
        {
            if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                setParameterCallback(query[key].ToArray());
            }
        }

        private static void TrySetStr(Action<string> setParameterCallback, string key, string value, IQueryCollection query)
        {
            if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                setParameterCallback(query[key].ToString());
            }
        }

        private static void TrySetInt(Action<int> setParameterCallback, string key, string value, IQueryCollection query)
        {
            if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                int number = GetInt(query[key].ToString(), key);
                setParameterCallback(number);
            }
        }

        private static bool TryGetToken<T>(JObject jObj, string key, out T result) where T: class
        {
            var token = jObj.SelectToken(key);
            if (token == null)
            {
                result = null;
                return false;
            }

            result = token as T;
            return result != null;
        }

        private static SortOrders GetSortOrder(string value)
        {
            SortOrders sortOrder;
            if (value.Equals(SortOrderNames.Ascending, StringComparison.CurrentCultureIgnoreCase))
            {
                sortOrder = SortOrders.Ascending;
            }
            else if (value.Equals(SortOrderNames.Descending, StringComparison.CurrentCultureIgnoreCase))
            {
                sortOrder = SortOrders.Descending;
            }
            else
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, Constants.SearchParameterNames.SortOrder));
            }

            return sortOrder;
        }

        private static int GetInt(string value, string name)
        {
            int number;
            if (!int.TryParse(value, out number))
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, name));
            }

            return number;
        }
    }
}
