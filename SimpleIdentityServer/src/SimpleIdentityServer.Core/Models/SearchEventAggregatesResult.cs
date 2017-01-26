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

using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Models
{
    public class GroupedEventAggregate
    {
        public GroupedEventAggregate(object key, IEnumerable<EventAggregate> evts)
        {
            Key = key;
            Events = evts;
        }

        public object Key { get; set; }
        public IEnumerable<EventAggregate> Events { get; set; }
    }

    public class SearchEventAggregatesResult
    {
        public SearchEventAggregatesResult(int totalResults, IEnumerable<EventAggregate> evts)
        {
            TotalResults = totalResults;
            Events = evts;
            IsGrouped = false;
        }

        public SearchEventAggregatesResult(int totalResults, IEnumerable<GroupedEventAggregate> groupedEvts)
        {
            TotalResults = totalResults;
            GroupedEvents = groupedEvts;
            IsGrouped = true;
        }

        public int TotalResults { get; private set; }
        public IEnumerable<EventAggregate> Events { get; private set; }
        public IEnumerable<GroupedEventAggregate> GroupedEvents { get; private set; }
        public bool IsGrouped { get; private set; }
    }
}
