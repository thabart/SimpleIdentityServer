using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Host;
    
namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.DiscoveryAction)]
    public class DiscoveryController : Controller
    {
        private readonly IDiscoveryActions _discoveryActions;

        public DiscoveryController(IDiscoveryActions discoveryActions)
        {
            _discoveryActions = discoveryActions;
        }

        public DiscoveryInformation Get()
        {
            return GetMetadata();
        }

        private DiscoveryInformation GetMetadata()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var authorizationEndPoint = issuer + "/" + Constants.EndPoints.Authorization;
            var tokenEndPoint = issuer + "/" + Constants.EndPoints.Token;
            var userInfoEndPoint = issuer + "/" + Constants.EndPoints.UserInfo;
            var jwksUri = issuer + "/" + Constants.EndPoints.Jwks;
            var registrationEndPoint = issuer + "/" + Constants.EndPoints.Registration;
            var revocationEndPoint = issuer + "/" + Constants.EndPoints.Revocation;
            // TODO : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html
            var checkSessionIframe = issuer + "/" + Constants.EndPoints.CheckSession;
            var endSessionEndPoint = issuer + "/" + Constants.EndPoints.EndSession;

            var result = _discoveryActions.CreateDiscoveryInformation();
            result.Issuer = issuer;
            result.AuthorizationEndPoint = authorizationEndPoint;
            result.TokenEndPoint = tokenEndPoint;
            result.UserInfoEndPoint = userInfoEndPoint;
            result.JwksUri = jwksUri;
            result.RegistrationEndPoint = registrationEndPoint;
            result.RevocationEndPoint = revocationEndPoint;
            result.Version = "1.0";

            result.CheckSessionEndPoint = checkSessionIframe;
            result.EndSessionEndPoint = endSessionEndPoint;

            return result;
        }
    }
}
