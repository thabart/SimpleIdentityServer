using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ConfigurationController)]
    public class ClientsController
    {
        private readonly IEndpointRepository _endpointRepository;

        public ClientsController(IEndpointRepository endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }

        [HttpGet("openid/{id}/{url?}")]
        public async Task<IActionResult> GetOpenidClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpDelete("openid/{id}/{url?}")]
        public async Task<IActionResult> DeleteOpenidClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;

        }

        [HttpPost("openid/.search")]
        public async Task<IActionResult> SearchOpenidClients([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return null;
        }

        [HttpGet("auth/{id}/{url?}")]
        public async Task<IActionResult> GetAuthClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpDelete("auth/{id}/{url?}")]
        public async Task<IActionResult> DeleteAuthClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;

        }

        [HttpPost("auth/.search")]
        public async Task<IActionResult> SearchAuthClients([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return null;
        }
    }
}
