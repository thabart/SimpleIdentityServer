using SimpleIdentityServer.Api.DTOs.Response;
using Swashbuckle.Swagger.Annotations;
using System.Web.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class RegistrationController : ApiController
    {
        [SwaggerOperation("RegisterOperation")]
        public ClientResponse Post(ClientResponse client)
        {
            return null;
        }
    }
}