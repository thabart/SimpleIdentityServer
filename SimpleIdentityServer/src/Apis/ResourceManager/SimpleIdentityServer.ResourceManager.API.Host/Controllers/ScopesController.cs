using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ScopesController)]
    public class ScopesController : Controller
    {
        public ScopesController()
        {

        }

        [HttpGet("openid/{id}/{url?}")]
        public async Task<IActionResult> GetOpenIdScope(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpDelete("openid/{id}/{url?}")]
        public async Task<IActionResult> DeleteOpenIdScope(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpPost("openid/.search")]
        public async Task<IActionResult> SearchOpenIdScopes([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return null;
        }

        [HttpGet("auth/{id}/{url?}")]
        public async Task<IActionResult> GetAuthScope(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpDelete("auth/{id}/{url?}")]
        public async Task<IActionResult> DeleteAuthScope(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpPost("auth/.search")]
        public async Task<IActionResult> SearchAuthScopes([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return null;
        }
    }
}
