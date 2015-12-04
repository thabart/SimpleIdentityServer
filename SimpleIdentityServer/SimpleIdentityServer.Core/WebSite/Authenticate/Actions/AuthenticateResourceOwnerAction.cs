using System.Security.Claims;

using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface IAuthenticateResourceOwnerAction
    {
        /// <summary>
        /// Returns an action result to the controller's action.
        /// 1). Redirect to the consent screen if the user is authenticated AND the request doesn't contain a login prompt.
        /// 2). Do nothing
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
        /// <param name="code">Encrypted parameter</param>
        /// <returns>Action result to the controller's action</returns>
        ActionResult Execute(
            AuthorizationParameter parameter,
            ClaimsPrincipal resourceOwnerPrincipal,
            string code);
    }

    public class AuthenticateResourceOwnerAction : IAuthenticateResourceOwnerAction
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        public AuthenticateResourceOwnerAction(
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory)
        {
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
        }


        /// <summary>
        /// Returns an action result to the controller's action.
        /// 1). Redirect to the consent screen if the user is authenticated AND the request doesn't contain a login prompt.
        /// 2). Do nothing
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
        /// <param name="code">Encrypted parameter</param>
        /// <returns>Action result to the controller's action</returns>
        public ActionResult Execute(
            AuthorizationParameter parameter,
            ClaimsPrincipal resourceOwnerPrincipal,
            string code)
        {
            var resourceOwnerIsAuthenticated = resourceOwnerPrincipal.IsAuthenticated();
            var promptParameters = _parameterParserHelper.ParsePromptParameters(parameter.Prompt);

            // 1).
            if (resourceOwnerIsAuthenticated && promptParameters != null && !promptParameters.Contains(PromptParameter.login))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
                result.RedirectInstruction.AddParameter("code", code);
                return result;
            }

            // 2).
            return _actionResultFactory.CreateAnEmptyActionResultWithNoEffect();
        }
    }
}
