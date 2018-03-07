using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.ResourceManager.Host.Stores;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Host.Controllers
{
    public class CacheController : Controller
    {
        private readonly ISimpleIdServerConfigurationClientFactory _simpleIdServerConfigurationClientFactory;
        private readonly IAccessTokenStore _accessTokenStore;

        public CacheController(ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            IAccessTokenStore accessTokenStore)
        {
            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _accessTokenStore = accessTokenStore;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await _accessTokenStore.GetAccessToken();
            return null;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return null;
        }
    }
}
