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

using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Host
{
    public class ScimOptions
    {
        public string EndPoint { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class OpenIdServerConfiguration
    {
        public List<ResourceOwner> Users { get; set; }
        public List<Core.Common.Models.Client> Clients { get; set; }
        public List<Translation> Translations { get; set; }
        public List<JsonWebKey> JsonWebKeys { get; set; }
    }

    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            Scim = new ScimOptions();
        }

        /// <summary>
        /// Scim options.
        /// </summary>
        public ScimOptions Scim { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        public OpenIdServerConfiguration Configuration { get; set; }
        /// <summary>
        /// Gets or sets the OAUTH configuration options.
        /// </summary>
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
    }
}
