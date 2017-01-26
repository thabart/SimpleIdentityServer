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

using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.EventStore.Host.Builders;
using SimpleIdentityServer.EventStore.Host.DTOs.Responses;
using SimpleIdentityServer.EventStore.Host.Extensions;
using SimpleIdentityServer.EventStore.Host.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EventStore.Host.Controllers
{
    [Route(Constants.RouteNames.Events)]
    public class EventsController : Controller
    {
        private readonly IEventAggregateRepository _repository;
        private readonly ISearchParameterParser _searchParameterParser;
        private readonly IHalLinkBuilder _halLinkBuilder;

        public EventsController(IEventAggregateRepository repository, 
            ISearchParameterParser searchParameterParser,
            IHalLinkBuilder halLinkBuilder)
        {
            _repository = repository;
            _searchParameterParser = searchParameterParser;
            _halLinkBuilder = halLinkBuilder;
        }

        [HttpGet(".search")]
        public async Task<ActionResult> Search()
        {
            var searchParameter = _searchParameterParser.ParseQuery(Request.Query);
            var result = await _repository.Search(searchParameter);
            var content = result.ToDto(searchParameter);
            FillHalInformation(content, result.Events);
            return new OkObjectResult(content);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            var result = await _repository.Get(id);
            if (result == null)
            {
                return new NotFoundResult();
            }

            var content = result.ToDto();
            FillHalInformation(content);
            return new OkObjectResult(content);
        }

        private void FillHalInformation(SearchResultResponse search, IEnumerable<EventAggregate> events)
        {
            var links = _halLinkBuilder.AddSelfLink($"/{Constants.RouteNames.Events}/.search").Build();
            search.Links = links;
            if (events != null)
            {
                search.Embedded = events.Select(e =>
                {
                    var dto = e.ToDto();
                    FillHalInformation(dto);
                    return dto;
                });
            }
        }

        private void FillHalInformation(EventResponse evt)
        {
            var links = _halLinkBuilder.AddSelfLink($"/{Constants.RouteNames.Events}/{evt.Id}").Build();
            evt.Links = links;
        }
    }
}
