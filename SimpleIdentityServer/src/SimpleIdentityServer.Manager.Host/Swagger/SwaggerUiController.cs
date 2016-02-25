using Microsoft.AspNet.Mvc;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Host.Swagger
{
    public class SwaggerUiController : Controller
    {
        [HttpGet("swagger/ui/index.html")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Swagger/index.cshtml", GetDiscoveryUrls());
        }

        private IDictionary<string, string> GetDiscoveryUrls()
        {
            return new Dictionary<string, string>()
            {
                { "V1", "/swagger/v1/swagger.json" }
            };
        }
    }
}
