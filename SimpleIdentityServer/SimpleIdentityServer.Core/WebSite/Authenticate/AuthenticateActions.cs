using System.Collections.Generic;
using System.Security.Claims;

using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.WebSite.Authenticate
{
    public interface IAuthenticateActions
    {
        ActionResult AuthenticateResourceOwner(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code);

        ActionResult LocalUserAuthentication(
            LocalAuthorizationParameter localAuthorizationParameter,
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code,
            out List<Claim> claims);
    }

    public class AuthenticateActions : IAuthenticateActions
    {
        private readonly IAuthenticateResourceOwnerAction _authenticateResourceOwnerAction;

        private readonly ILocalUserAuthenticationAction _localUserAuthenticationAction;

        public AuthenticateActions(
            IAuthenticateResourceOwnerAction authenticateResourceOwnerAction,
            ILocalUserAuthenticationAction localUserAuthenticationAction)
        {
            _authenticateResourceOwnerAction = authenticateResourceOwnerAction;
            _localUserAuthenticationAction = localUserAuthenticationAction;
        }

        public ActionResult AuthenticateResourceOwner(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code)
        {
            return _authenticateResourceOwnerAction.Execute(parameter, claimsPrincipal, code);
        }

        public ActionResult LocalUserAuthentication(
            LocalAuthorizationParameter localAuthorizationParameter,
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code,
            out List<Claim> claims)
        {
            return _localUserAuthenticationAction.Execute(
                localAuthorizationParameter,
                parameter,
                claimsPrincipal,
                code,
                out claims);
        }
    }
}
