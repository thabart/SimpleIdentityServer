using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ConfigurationController)]
    public class ConfigurationController : Controller
    {
        public ActionResult Get()
        {
            var json = new JObject();
            json.Add("version", "2.1");
            return new OkObjectResult(json);
        }
    }
}
