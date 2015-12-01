using System.Collections.Generic;
using System.Security.Claims;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.WebSite.Consent
{
    public interface IConsentActions
    {
        ActionResult DisplayConsent(
            AuthorizationParameter authorizationParameter,
            out Client client,
            out List<Scope> allowedScopes,
            out List<string> allowedClaims);

        ActionResult ConfirmConsent(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal);
    }

    public class ConsentActions : IConsentActions
    {
        private readonly IDisplayConsentAction _displayConsentAction;

        private readonly IConfirmConsentAction _confirmConsentAction;

        public ConsentActions(
            IDisplayConsentAction displayConsentAction,
            IConfirmConsentAction confirmConsentAction)
        {
            _displayConsentAction = displayConsentAction;
            _confirmConsentAction = confirmConsentAction;
        }

        public ActionResult DisplayConsent(
            AuthorizationParameter authorizationParameter,
            out Client client,
            out List<Scope> allowedScopes,
            out List<string> allowedClaims)
        {
            return _displayConsentAction.Execute(authorizationParameter,
                out client,
                out allowedScopes,
                out allowedClaims);
        }

        public ActionResult ConfirmConsent(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            return _confirmConsentAction.Execute(authorizationParameter,
                claimsPrincipal);
        }
    }
}
