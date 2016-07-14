using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Xml;

namespace WsFederation
{
    public class WsFedAuthenticationOptions : AuthenticationOptions
    {
        #region Constructor

        public WsFedAuthenticationOptions()
        {
            CallbackPath = new PathString("/signin-adfs");
        }

        #endregion

        #region Properties

        public string Realm { get; set; }

        public string IdPEndpoint { get; set; }

        public string RedirectUrl { get; set; }

        public PathString CallbackPath { get; set; }

        public PathString RedirectPath { get; set; }

        public string SignInScheme { get; set; }

        public string DisplayName { get; set; }

        public Func<XmlNodeList, List<Claim>> GetClaimsCallback { get; set; }

        #endregion
    }
}
