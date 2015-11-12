using System;
using System.Web.Http;

using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class DiscoveryController : ApiController
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
            var authorizationEndPoint = new Uri(issuer, Constants.EndPoints.Authorization);
            var tokenEndPoint = new Uri(issuer, Constants.EndPoints.Token);
            var userInfoEndPoint = new Uri(issuer, Constants.EndPoints.UserInfo);
            var jwksUri = new Uri(issuer, Constants.EndPoints.Jwks);
            var registrationEndPoint = new Uri(issuer, Constants.EndPoints.Registration);

            var result = _discoveryActions.CreateDiscoveryInformation();
            result.Issuer = issuer.AbsoluteUri;
            result.AuthorizationEndPoint = authorizationEndPoint.AbsoluteUri;
            result.TokenEndPoint = tokenEndPoint.AbsoluteUri;
            result.UserInfoEndPoint = userInfoEndPoint.AbsoluteUri;
            result.JwksUri = jwksUri.AbsoluteUri;
            result.RegistrationEndPoint = registrationEndPoint.AbsoluteUri;

            return result;
        }
    }
}
