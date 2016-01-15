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
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token
    /// </summary>
    [KnownType(typeof(object[]))]
    [KnownType(typeof(string[]))]
    public class JwsPayload : Dictionary<string, object>
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        public string Issuer
        {
            get { return GetStringClaim(Constants.StandardClaimNames.Issuer); }
        }

        /// <summary>
        /// Gets or sets the audience(s)
        /// </summary>
        public string[] Audiences
        {
            get { return GetArrayClaim(Constants.StandardClaimNames.Audiences); }
        }

        /// <summary>
        /// Gets or sets the expiration time
        /// </summary>
        public double ExpirationTime
        {
            get { return GetDoubleClaim(Constants.StandardClaimNames.ExpirationTime); }
        }

        /// <summary>
        /// Gets or sets the IAT
        /// </summary>
        public double Iat
        {
            get { return GetDoubleClaim(Constants.StandardClaimNames.Iat); }
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public string Jti
        {
            get
            {
                return GetStringClaim(Constants.StandardClaimNames.Jti);
            }
        }

        /// <summary>
        /// Gets or sets the authentication time
        /// </summary>
        public double AuthenticationTime
        {
            get { return GetDoubleClaim(Constants.StandardClaimNames.AuthenticationTime); }
        }

        /// <summary>
        /// Gets or sets the NONCE
        /// </summary>
        public string Nonce
        {
            get { return GetStringClaim(Constants.StandardClaimNames.Nonce); }
        }

        /// <summary>
        /// Gets or sets the authentication context class reference
        /// </summary>
        public string Acr
        {
            get { return GetStringClaim(Constants.StandardClaimNames.Acr); }
        }

        /// <summary>
        /// Gets or sets the Authentication Methods References
        /// </summary>
        [DataMember(Name = "amr")]
        public string Amr
        {
            get { return GetStringClaim(Constants.StandardClaimNames.Amr); }
        }

        /// <summary>
        /// Gets or sets the Authorized party
        /// </summary>
        [DataMember(Name = "azp")]
        public string Azp
        {
            get { return GetStringClaim(Constants.StandardClaimNames.Azp); }
        }

        public string GetClaimValue(string claimName)
        {
            if (!ContainsKey(claimName) || this[claimName] == null)
            {
                return null;
            }

            return this[claimName].ToString();
        }

        private string GetStringClaim(string claimName)
        {
            if (!ContainsKey(claimName))
            {
                return null;
            }

            return this[claimName].ToString();
        }

        public double GetDoubleClaim(string claimName)
        {
            if (!ContainsKey(claimName))
            {
                return default(double);
            }

            double result;
            var claim = this[claimName].ToString();
            if (double.TryParse(claim, out result))
            {
                return result;
            }

            return default(double);
        }

        private string[] GetArrayClaim(string claimName)
        {
            if (!ContainsKey(claimName))
            {
                return new string[0];
            }

            var claim = this[claimName];
            var type = claim.GetType();
            if (!type.IsArray)
            {
                return new string[0];
            }

            return ((object[]) claim).Select(c => c.ToString()).ToArray();
        }
    }
}
