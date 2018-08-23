using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Scim.Mapping.Ad.Extensions;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Controllers
{
    [Route(Constants.RouteNames.AdConfigurationController)]
    public class AdConfigurationController : Controller
    {
        private readonly IConfigurationStore _configurationStore;

        public AdConfigurationController(IConfigurationStore configurationStore)
        {
            _configurationStore = configurationStore;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _configurationStore.GetConfiguration();
            return new OkObjectResult(result.ToDto());
        }
    }
}