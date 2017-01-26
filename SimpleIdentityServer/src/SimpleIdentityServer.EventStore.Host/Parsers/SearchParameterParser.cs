#region copyright
// Copyright 2017 Habart Thierry
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
using SimpleIdentityServer.Core.Parameters;
using System;

namespace SimpleIdentityServer.EventStore.Host.Parsers
{
    public interface ISearchParameterParser
    {
        SearchParameter ParseQuery(IQueryCollection query);
    }

    internal class SearchParameterParser : ISearchParameterParser
    {
        public SearchParameter ParseQuery(IQueryCollection query)
        {
            var result = new SearchParameter();
            if (query == null)
            {
                return result;
            }

            foreach (var key in query.Keys)
            {
                TrySetStr((r) => result.SortBy = r, key, Constants.SearchParameterNames.SortBy, query);
                TrySetStr((r) => result.SortOrder = GetSortOrder(r), key, Constants.SearchParameterNames.SortOrder, query);
                TrySetStr((r) => result.Filter = r, key, Constants.SearchParameterNames.Filter, query);
                TrySetStr((r) => result.GroupBy = r, key, Constants.SearchParameterNames.GroupBy, query);
                TrySetInt((r) => result.StartIndex = r <= 0 ? result.StartIndex : r, key, Constants.SearchParameterNames.StartIndex, query);
                TrySetInt((r) => result.Count = r <= 0 ? result.Count : r, key, Constants.SearchParameterNames.Count, query);
            }

            return result;
        }

        private static SortOrders GetSortOrder(string value)
        {
            SortOrders sortOrder;
            if (value.Equals(Constants.SortOrderNames.Ascending, StringComparison.CurrentCultureIgnoreCase))
            {
                sortOrder = SortOrders.Ascending;
            }
            else if (value.Equals(Constants.SortOrderNames.Descending, StringComparison.CurrentCultureIgnoreCase))
            {
                sortOrder = SortOrders.Descending;
            }
            else
            {
                throw new InvalidOperationException($"the parameter {Constants.SearchParameterNames.SortOrder} is not valid");
            }

            return sortOrder;
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

        private static int GetInt(string value, string name)
        {
            int number;
            if (!int.TryParse(value, out number))
            {
                throw new InvalidOperationException($"the parameter {name} is not valid");
            }

            return number;
        }
    }
}
