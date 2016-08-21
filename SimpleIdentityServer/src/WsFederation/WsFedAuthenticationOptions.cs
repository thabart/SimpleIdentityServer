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
            Events = new WsFedAuthenticationEvents();
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

        public IWsFedAuthenticationEvents Events { get; set; }

        public Func<XmlNodeList, List<Claim>> GetClaimsCallback { get; set; }

        #endregion
    }
}
