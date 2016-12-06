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

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class GrantedToken
    {
        public string Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime CreateDateTime { get; set; }
        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Gets or sets the user information payload
        /// </summary>
        public string UserInfoPayLoad { get; set; }
        /// <summary>
        /// Gets or sets the identity token payload
        /// </summary>
        public string IdTokenPayLoad { get; set; }
        public string ParentTokenId { get; set; }
        public virtual GrantedToken Parent { get; set; }
        public virtual IList<GrantedToken> Children { get; set; }
    }
}
