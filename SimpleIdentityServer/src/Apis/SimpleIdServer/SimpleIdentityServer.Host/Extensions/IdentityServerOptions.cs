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

using Microsoft.AspNetCore.Authentication.Cookies;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Host
{
    public class AuthenticateOptions
    {
        public string CookieName = CookieAuthenticationDefaults.AuthenticationScheme;
    }

    public class ScimOptions
    {
        public string EndPoint { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class EventOptions
    {
        public Type Publisher { get; set; }
        public IEnumerable<Type> Handlers { get; set; }
    }

    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            Authenticate = new AuthenticateOptions();
            Scim = new ScimOptions();
        }

        /// <summary>
        /// Configure authentication.
        /// </summary>
        public AuthenticateOptions Authenticate { get; set; }
        /// <summary>
        /// Scim options.
        /// </summary>
        public ScimOptions Scim { get; set; }
        /// <summary>
        /// Configure the event publisher &|or handlers.
        /// </summary>
        public EventOptions Event { get; set; }
        /// <summary>
        /// Service used to authenticate the resource owner.
        /// </summary>
        public Type AuthenticateResourceOwner { get; set; }
        /// <summary>
        /// Service used to retrieve configurations (expiration date time etc ...)
        /// </summary>
        public Type ConfigurationService { get; set; }
        /// <summary>
        /// Service used to encrypt the password
        /// </summary>
        public Type PasswordService { get; set; }
        /// <summary>
        /// Store the two factor authentication methods.
        /// </summary>
        public ITwoFactorServiceStore TwoFactorServiceStore { get; set; }
    }
}
