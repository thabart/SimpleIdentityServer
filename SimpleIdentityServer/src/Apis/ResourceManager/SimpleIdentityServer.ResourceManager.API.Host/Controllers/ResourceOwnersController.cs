using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ResourceOwnersController)]
    public class ResourceOwnersController
    {
        public ResourceOwnersController()
        {

        }

        [HttpGet("{id}/{url?}")]
        public async Task<IActionResult> Get(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpDelete("{id}/{url?}")]
        public async Task<IActionResult> Delete(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpPost(".search")]
        public async Task<IActionResult> Search([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return null;
        }
    }
}
