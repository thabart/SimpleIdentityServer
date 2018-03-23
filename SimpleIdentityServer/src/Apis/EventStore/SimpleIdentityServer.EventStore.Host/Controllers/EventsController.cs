using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.EventStore.Host.Builders;
using SimpleIdentityServer.EventStore.Host.Extensions;
using SimpleIdentityServer.EventStore.Host.Parsers;
using System.Threading.Tasks;
using SimpleIdentityServer.EventStore.Core.Repositories;

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

        private void FillHalInformation(JObject evt, string id)
        {
            var links = _halLinkBuilder.AddSelfLink($"/{Constants.RouteNames.Events}/{id}").Build();
            evt.Add(new JProperty(Constants.HalResponseNames.Links, links));
        }
    }
}
