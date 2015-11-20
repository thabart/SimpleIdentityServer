using System.Web.Http;

using SimpleIdentityServer.Api.DTOs.Response;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class JwksController : ApiController
    {
        public JwksController()
        {
            
        }

        public JsonWebKeySet Get()
        {
            return null;
        }
    }
}
