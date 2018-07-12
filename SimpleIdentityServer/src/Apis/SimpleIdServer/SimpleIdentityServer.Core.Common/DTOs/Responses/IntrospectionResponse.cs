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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs.Responses
{
    [DataContract]
    public class IntrospectionResponse
    {
        /// <summary>
        /// Gets or sets a boolean indicator of whether or not the presented token is currently active
        /// </summary>
        [DataMember(Name = IntrospectionNames.Active)]
        public bool Active { get; set; }
        /// <summary>
        /// Gets or sets a list of scopes
        /// </summary>
        [DataMember(Name = IntrospectionNames.Scope)]
        public IEnumerable<string> Scope { get; set; }
        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        [DataMember(Name = IntrospectionNames.ClientId)]
        public string ClientId { get; set; }
        /// <summary>
        /// Gets or sets identifier for the resource owner who authorized this token
        /// </summary>
        [DataMember(Name = IntrospectionNames.UserName)]
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the token type
        /// </summary>
        [DataMember(Name = IntrospectionNames.TokenType)]
        public string TokenType { get; set; }
        /// <summary>
        /// Gets or sets the expiration in seconds
        /// </summary>
        [DataMember(Name = IntrospectionNames.Expiration)]
        public int Expiration { get; set; }
        /// <summary>
        /// Gets or sets the issue date
        /// </summary>
        [DataMember(Name = IntrospectionNames.IssuedAt)]
        public double IssuedAt { get; set; }
        /// <summary>
        /// Gets or sets the NBF
        /// </summary>
        [DataMember(Name = IntrospectionNames.Nbf)]
        public int Nbf { get; set; }
        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        [DataMember(Name = IntrospectionNames.Subject)]
        public string Subject { get; set; }
        /// <summary>
        /// Gets or sets the audience
        /// </summary>
        [DataMember(Name = IntrospectionNames.Audience)]
        public string Audience { get; set; }
        /// <summary>
        /// Gets or sets the issuer of this token
        /// </summary>
        [DataMember(Name = IntrospectionNames.Issuer)]
        public string Issuer { get; set; }
        /// <summary>
        /// Gets or sets the string representing the issuer of the token
        /// </summary>
        [DataMember(Name = IntrospectionNames.Jti)]
        public string Jti { get; set; }
    }
}
