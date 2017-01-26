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
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.EventStore.Host.Builders;
using SimpleIdentityServer.EventStore.Host.Extensions;
using SimpleIdentityServer.EventStore.Host.Parsers;
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
            FillHalInformation(content, result);
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
            FillHalInformation(content, result.Id);
            return new OkObjectResult(content);
        }

        private void FillHalInformation(JObject search, SearchEventAggregatesResult result)
        {
            var links = _halLinkBuilder.AddSelfLink($"/{Constants.RouteNames.Events}/.search").Build();
            search.Add(new JProperty(Constants.HalResponseNames.Links, links));
            JArray evtArr = null;
            if (result.IsGrouped)
            {
                if (result.GroupedEvents != null)
                {
                    var embeddedContent = new JObject();
                    foreach(var groupedEvt in result.GroupedEvents)
                    {
                        evtArr = new JArray();
                        foreach (var evt in groupedEvt.Events)
                        {
                            var dto = evt.ToDto();
                            FillHalInformation(dto, evt.Id);
                            evtArr.Add(dto);
                        }
                        embeddedContent.Add(new JProperty(groupedEvt.Key.ToString(), evtArr));
                    }

                    search.Add(new JProperty(Constants.HalResponseNames.Embedded, embeddedContent));
                }

                return;
            }

            if (result.Events != null)
            {
                evtArr = new JArray();
                foreach (var evt in result.Events)
                {
                    var dto = evt.ToDto();
                    FillHalInformation(dto, evt.Id);
                    evtArr.Add(dto);
                }

                search.Add(new JProperty(Constants.HalResponseNames.Embedded, evtArr));
            }
        }

        private void FillHalInformation(JObject evt, string id)
        {
            var links = _halLinkBuilder.AddSelfLink($"/{Constants.RouteNames.Events}/{id}").Build();
            evt.Add(new JProperty(Constants.HalResponseNames.Links, links));
        }
    }
}
