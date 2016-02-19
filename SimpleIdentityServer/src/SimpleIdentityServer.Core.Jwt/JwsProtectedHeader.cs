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

using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt
{
    [DataContract]
    public class JwsProtectedHeader
    {
        /// <summary>
        /// Gets or sets the encoded object type. In general its value is JWT
        /// </summary>
        [DataMember(Name = "typ")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used to secure the JwsProtectedHeader & the JWS payload.
        /// </summary>
        [DataMember(Name = "alg")]
        public string Alg { get; set; }

        /// <summary>
        /// Gets or sets the identifier indicating the key that was used to secure the token.
        /// </summary>
        [DataMember(Name = "kid")]
        public string Kid { get; set; }
    }
}
