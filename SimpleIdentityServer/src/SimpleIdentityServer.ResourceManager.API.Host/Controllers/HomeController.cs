using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var json = new JObject();
            json.Add("version", "2.1");
            return new OkObjectResult(json);
        }
    }
}
