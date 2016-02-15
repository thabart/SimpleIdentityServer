using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Core.Api.Jwks;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Host;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.Jwks)]
    public class JwksController : Controller
    {
        private readonly IJwksActions _jwksActions;

        public JwksController(IJwksActions jwksActions)
        {
            _jwksActions = jwksActions;
        }

        [HttpGet]
        public JsonWebKeySet Get()
        {
            var jsonWebKeySet = _jwksActions.GetJwks();
            return jsonWebKeySet;
        }

        [HttpPut]
        public bool Put()
        {
            return _jwksActions.RotateJwks();
        }
    }
}
