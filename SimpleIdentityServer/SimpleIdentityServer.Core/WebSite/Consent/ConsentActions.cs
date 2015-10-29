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
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            out Client client,
            out List<Scope> allowedScopes);

        ActionResult ConfirmConsent(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
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
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            out Client client,
            out List<Scope> allowedScopes)
        {
            return _displayConsentAction.Execute(authorizationCodeGrantTypeParameter,
                out client,
                out allowedScopes);
        }

        public ActionResult ConfirmConsent(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            return _confirmConsentAction.Execute(authorizationCodeGrantTypeParameter,
                claimsPrincipal);
        }
    }
}
