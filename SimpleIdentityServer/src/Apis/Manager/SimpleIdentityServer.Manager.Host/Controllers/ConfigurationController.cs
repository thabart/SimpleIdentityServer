using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Host.Extensions;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Configuration)]
    public class ConfigurationController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var jObj = new JObject();
            jObj.Add(Constants.ConfigurationNames.ClientsEndpoint, issuer + Constants.EndPoints.Clients);
            jObj.Add(Constants.ConfigurationNames.JweEndpoint, issuer + Constants.EndPoints.Jwe);
            jObj.Add(Constants.ConfigurationNames.JwsEndpoint, issuer + Constants.EndPoints.Jws);
            jObj.Add(Constants.ConfigurationNames.ManageEndpoint, issuer + Constants.EndPoints.Manage);
            jObj.Add(Constants.ConfigurationNames.ResourceOwnersEndpoint, issuer + Constants.EndPoints.ResourceOwners);
            jObj.Add(Constants.ConfigurationNames.ScopesEndpoint, issuer + Constants.EndPoints.Scopes);
            jObj.Add(Constants.ConfigurationNames.ClaimsEndpoint, issuer + Constants.EndPoints.Scopes);
            return new OkObjectResult(jObj);
        }
    }
}
