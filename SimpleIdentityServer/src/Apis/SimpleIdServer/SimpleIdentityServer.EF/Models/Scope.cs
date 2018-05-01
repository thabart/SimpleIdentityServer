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

namespace SimpleIdentityServer.EF.Models
{
    public enum ScopeType
    {
        ProtectedApi,
        ResourceOwner
    }

    public class Scope
    {
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether the scope is an open-id scope.
        /// </summary>
        public bool IsOpenIdScope { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether the scope will be displayed in the consent screen or not.
        /// </summary>
        public bool IsDisplayedInConsent { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether the scope will be displayed in the well-known configuration endpoint.
        /// </summary>
        public bool IsExposed { get; set; }

        public ScopeType Type { get; set; }

        public virtual List<ScopeClaim> ScopeClaims { get; set; }

        public virtual List<ClientScope> ClientScopes { get; set; }

        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual List<ConsentScope> ConsentScopes { get; set; } 

        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
