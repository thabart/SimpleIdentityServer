#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Consent
{
    public interface IConsentActions
    {
        Task<DisplayContentResult> DisplayConsent(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName);
        Task<ActionResult> ConfirmConsent(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName);
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

        public async Task<DisplayContentResult> DisplayConsent(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            return await _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, issuerName);
        }

        public async Task<ActionResult> ConfirmConsent(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            return await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, issuerName);
        }
    }
}
