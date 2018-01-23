using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.ResourceManager.Host.DTOs;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Host.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        public HomeController(IConfiguration configuration, IIdentityServerClientFactory identityServerClientFactory)
        {
            _configuration = configuration;
            _identityServerClientFactory = identityServerClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        /// <summary>
        /// Authenticate with login and password.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] JObject json)
        {
            if (json == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(Constants.ErrorCodes.IncompleteRequest, "The login and password must be specified").GetJson());
            }

            JToken jLogin;
            if (!json.TryGetValue(Constants.AuthenticateDtoNames.Login, out jLogin))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(Constants.ErrorCodes.IncompleteRequest, "the login must be specified").GetJson());
            }

            JToken jPassword;
            if (!json.TryGetValue(Constants.AuthenticateDtoNames.Password, out jPassword))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(Constants.ErrorCodes.IncompleteRequest, "the password must be specified").GetJson());
            }

            var wellKnownConfiguration = _configuration["OpenId:WellKnownConfiguration"];
            if (string.IsNullOrWhiteSpace(wellKnownConfiguration))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(Constants.ErrorCodes.InvalidConfiguration, "the well-known configuration is not specified").GetJson());
            }

            GrantedToken grantedToken = null;
            try
            {
                grantedToken = await _identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretPostAuth(Constants.ClientId, Constants.ClientSecret)
                    .UsePassword(jLogin.ToString(), jPassword.ToString(), "openid", "profile")
                    .ResolveAsync(wellKnownConfiguration);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse(Constants.ErrorCodes.InvalidRequest, "An error occured while trying to authenticate the user").GetJson());
            }

            var result = new JObject();
            result.Add(Constants.GrantedTokenNames.AccessToken, grantedToken.AccessToken);
            result.Add(Constants.GrantedTokenNames.IdToken, grantedToken.IdToken);
            result.Add(Constants.GrantedTokenNames.ExpiresIn, grantedToken.ExpiresIn);
            result.Add(Constants.GrantedTokenNames.RefreshToken, grantedToken.RefreshToken);
            result.Add(Constants.GrantedTokenNames.Scope, new JArray(grantedToken.Scope));
            result.Add(Constants.GrantedTokenNames.TokenType, grantedToken.TokenType);
            return new OkObjectResult(result);
        }
    }
}
