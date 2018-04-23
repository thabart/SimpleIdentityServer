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

using System;

namespace SimpleIdentityServer.Uma.Host.Configurations
{
    public class UmaHostConfiguration
    {
        /// <summary>
        /// Get or set the OPENID well known configuration.
        /// </summary>
        public string OpenIdWellKnownConfiguration { get; set; }
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
    }
}
