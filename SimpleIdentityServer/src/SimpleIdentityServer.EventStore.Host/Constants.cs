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

namespace SimpleIdentityServer.EventStore.Host
{
    public class Constants
    {
        public static class RouteNames
        {
            public const string Events = "events";
        }

        public static class SearchParameterNames
        {
            public const string SortBy = "sortBy";
            public const string SortOrder = "sortOrder";
            public const string StartIndex = "startIndex";
            public const string Count = "count";
            public const string Filter = "filter";
            public const string GroupBy = "groupby";
        }

        public static class SearchResultResponseNames
        {
            public const string TotalResult = "totalResults";
            public const string ItemsPerPage = "itemsPerPage";
            public const string StartIndex = "startIndex";
        }

        public static class SortOrderNames
        {
            public static string Ascending = "ascending";
            public static string Descending = "descending";
        }

        public static class HalResponseNames
        {
            public const string Links = "_links";
            public const string Embedded = "_embedded";
        }

        public static class HalLinkResponseNames
        {
            public const string Self = "self";
        }

        public static class HalLinkPropertyResponseNames
        {
            public const string Href = "href";
            public const string Name = "name";
            public const string Templated = "templated";
        }
    }
}
