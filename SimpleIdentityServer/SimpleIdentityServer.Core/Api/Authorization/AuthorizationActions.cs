using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Api.Authorization
{
    public interface IAuthorizationActions
    {
        ActionResult GetAuthorization(AuthorizationCodeGrantTypeParameter parameter,
            IPrincipal claimsPrincipal,
            string code);
    }

    public class AuthorizationActions : IAuthorizationActions
    {
        private readonly IGetAuthorizationCodeOperation _getAuthorizationCodeOperation;

        public AuthorizationActions(IGetAuthorizationCodeOperation getAuthorizationCodeOperation)
        {
            _getAuthorizationCodeOperation = getAuthorizationCodeOperation;
        }

        public ActionResult GetAuthorization(AuthorizationCodeGrantTypeParameter parameter,
            IPrincipal claimsPrincipal,
            string code)
        {
            return _getAuthorizationCodeOperation.Execute(
                parameter, 
                claimsPrincipal,
                code);
        }
    }
}
