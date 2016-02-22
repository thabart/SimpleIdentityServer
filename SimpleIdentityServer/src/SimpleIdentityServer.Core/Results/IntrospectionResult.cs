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

namespace SimpleIdentityServer.Core.Results
{
    public class IntrospectionResult
    {
        /// <summary>
        /// Gets or sets a boolean indicator of whether or not the presented token is currently active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a list of scopes
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        public string ClientId { get; set; }


        /// <summary>
        /// Gets or sets identifier for the resource owner who authorized this token
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// Gets or sets the token type
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the expiration in seconds
        /// </summary>
        public int Expiration { get; set; }


        /// <summary>
        /// Gets or sets the issue date
        /// </summary>
        public double IssuedAt { get; set; }

        /// <summary>
        /// Gets or sets the NBF
        /// </summary>
        public int Nbf { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the audience
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the issuer of this token
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the string representing the issuer of the token
        /// </summary>
        public string Jti { get; set; }
    }
}
