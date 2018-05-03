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
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Common.Models
{
    public class ResourceOwner
    {
        /// <summary>
        /// Get or sets the subject-identifier for the End-User at the issuer.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the list of claims.
        /// </summary>
        public IList<Claim> Claims { get; set; }
        /// <summary>
        /// Gets or sets the two factor authentications
        /// </summary>
        public TwoFactorAuthentications TwoFactorAuthentication { get; set; }
        /// <summary>
        /// Gets or sets if the resource owner is local or external
        /// </summary>
        public bool IsLocalAccount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }

    public enum TwoFactorAuthentications
    {
        NONE,
        Email,
        Sms
    }
}