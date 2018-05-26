#region copyright
// Copyright 2016 Habart Thierry
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

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Client
{
    public enum SortOrders
    {
        [EnumMember(Value = Common.Constants.SortOrderNames.Ascending)]
        Ascending,
        [EnumMember(Value = Common.Constants.SortOrderNames.Descending)]
        Descending
    }

    [DataContract]
    public class SearchParameter
    {
        [DataMember(Name = Common.Constants.SearchParameterNames.Attributes)]
        public IEnumerable<string> Attributes { get; set; }

        [DataMember(Name = Common.Constants.SearchParameterNames.ExcludedAttributes)]
        public IEnumerable<string> ExcludedAttributes { get; set; }

        [DataMember(Name = Common.Constants.SearchParameterNames.Filter)]
        public string Filter { get; set; }

        [DataMember(Name = Common.Constants.SearchParameterNames.SortBy)]
        public string SortBy { get; set; }

        [DataMember(Name = Common.Constants.SearchParameterNames.SortOrder)]
        public SortOrders? SortOrder { get; set; }

        [DataMember(Name = Common.Constants.SearchParameterNames.StartIndex)]
        public int? StartIndex { get; set; }

        [DataMember(Name = Common.Constants.SearchParameterNames.Count)]
        public int? Count { get; set; }

        public string ToJson()
        {
            var obj = new JObject();
            if (Attributes != null && Attributes.Any())
            {
                obj[Common.Constants.SearchParameterNames.Attributes] = new JArray(Attributes);
            }

            if (ExcludedAttributes != null && ExcludedAttributes.Any())
            {
                obj[Common.Constants.SearchParameterNames.ExcludedAttributes] = new JArray(ExcludedAttributes);
            }

            if (!string.IsNullOrEmpty(Filter))
            {
                obj[Common.Constants.SearchParameterNames.Filter] = Filter;
            }

            if (!string.IsNullOrEmpty(SortBy))
            {
                obj[Common.Constants.SearchParameterNames.SortBy] = SortBy;
            }

            if (SortOrder != null)
            {
                obj[Common.Constants.SearchParameterNames.SortOrder] = SortOrder == SortOrders.Ascending
                    ? Common.Constants.SortOrderNames.Ascending
                    : Common.Constants.SortOrderNames.Descending;
            }

            if (StartIndex != null)
            {
                obj[Common.Constants.SearchParameterNames.StartIndex] = StartIndex;
            }

            if (Count != null)
            {
                obj[Common.Constants.SearchParameterNames.Count] = Count;
            }

            return obj.ToString();
        }
    }
}
