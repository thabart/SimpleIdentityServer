using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features.Authentication;
using WsFederation.Messages;
using System.Security.Claims;

namespace WsFederation
{
    public class WsFedSignInContext : SignInContext
    {
        #region Constructor

        public WsFedSignInContext(
            string authenticationScheme,
            ClaimsPrincipal principal, 
            IDictionary<string, string> properties, 
            SignInResponseMessage signInMessage, 
            string returnUrl) : base(authenticationScheme, principal, properties)
        {
            SignInMessage = signInMessage;
            ReturnUrl = returnUrl;
        }

        #endregion

        #region Properties

        public SignInResponseMessage SignInMessage { get; private set; }

        public string ReturnUrl { get; private set; }

        #endregion
    }
}
