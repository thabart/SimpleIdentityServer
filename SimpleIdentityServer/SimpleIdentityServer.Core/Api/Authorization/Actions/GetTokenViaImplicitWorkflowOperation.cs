using System.Security.Principal;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetTokenViaImplicitWorkflowOperation
    {
        ActionResult Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            IPrincipal principal,
            string code);
    }

    public class GetTokenViaImplicitWorkflowOperation : IGetTokenViaImplicitWorkflowOperation
    {
        public ActionResult Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            IPrincipal principal,
            string code)
        {
            if (string.IsNullOrWhiteSpace(authorizationCodeGrantTypeParameter.Nonce))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "scope"),
                    authorizationCodeGrantTypeParameter.State);
            }

            return null;
        }
    }
}
