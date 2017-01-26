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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using System;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    internal static class MappingExtensions
    {
        public static JObject ToDto(this EventAggregate evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            var result = new JObject();
            result.Add(new JProperty(Core.Common.EventResponseNames.Id, evt.Id));
            result.Add(new JProperty(Core.Common.EventResponseNames.AggregateId, evt.AggregateId));
            result.Add(new JProperty(Core.Common.EventResponseNames.Description, evt.Description));
            result.Add(new JProperty(Core.Common.EventResponseNames.CreatedOn, evt.CreatedOn));
            result.Add(new JProperty(Core.Common.EventResponseNames.Payload, evt.Payload));
            return result;
        }

        public static JObject ToDto(this SearchEventAggregatesResult search, SearchParameter parameter)
        {
            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = new JObject();
            result.Add(new JProperty(Constants.SearchResultResponseNames.TotalResult, search.TotalResults));
            result.Add(new JProperty(Constants.SearchResultResponseNames.ItemsPerPage, parameter.Count));
            result.Add(new JProperty(Constants.SearchResultResponseNames.StartIndex, parameter.StartIndex));
            return result;
        }
    }
}
