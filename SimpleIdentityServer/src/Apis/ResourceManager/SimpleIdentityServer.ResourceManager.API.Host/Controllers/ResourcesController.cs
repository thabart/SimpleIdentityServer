using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.Core.Api.Resources;
using SimpleIdentityServer.ResourceManager.Core.Exceptions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ResourcesController)]
    public class ResourcesController : Controller
    {
        private readonly IResourcesetActions _resourcesetActions;

        public ResourcesController(IResourcesetActions resourcesetActions)
        {
            _resourcesetActions = resourcesetActions;
        }

        [HttpPost(".search/{url?}")]
        public async Task<IActionResult> Search([FromBody] SearchResourceSet parameter, string url)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            try
            {
                var result = await _resourcesetActions.Search(url, parameter);
                return new OkObjectResult(result);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
