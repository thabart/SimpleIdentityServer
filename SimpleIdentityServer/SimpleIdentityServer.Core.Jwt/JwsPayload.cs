using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token
    /// </summary>
    [KnownType(typeof(object[]))]
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
            if (!ContainsKey(claimName))
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

        private double GetDoubleClaim(string claimName)
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
