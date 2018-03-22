using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.EventStore.Host.Extensions;

namespace SimpleIdentityServer.EventStore.Host.Controllers
{
    [Route(Constants.RouteNames.Discovery)]
    public class DiscoveryController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var eventsEndpoint = issuer + "/" + Constants.RouteNames.Events;
            var jObj = new JObject();
            jObj.Add(Constants.ConfigurationParameterNames.Version, Constants.Version);
            jObj.Add(Constants.ConfigurationParameterNames.EventsEndpoint, eventsEndpoint);
            return new OkObjectResult(jObj);
        }
    }
}
