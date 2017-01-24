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
using SimpleIdentityServer.EventStore.Host.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.EventStore.Host.Extensions
{
    internal static class MappingExtensions
    {
        public static EventResponse ToDto(this EventAggregate evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            return new EventResponse
            {
                Id = evt.Id,
                AggregateId = evt.AggregateId,
                Description = evt.Description,
                CreatedOn = evt.CreatedOn,
                Payload = evt.Payload
            };
        }

        public static SearchResultResponse ToDto(this SearchEventAggregatesResult search, SearchParameter parameter)
        {
            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<EventResponse> resources = new List<EventResponse>();
            if (search.Events != null)
            {
                resources = search.Events.Select(e => e.ToDto());
            }

            return new SearchResultResponse
            {
                TotalResult = search.TotalResults,
                Resources = resources,
                ItemsPerPage = parameter.Count,
                StartIndex = parameter.StartIndex
            };
        }
    }
}
