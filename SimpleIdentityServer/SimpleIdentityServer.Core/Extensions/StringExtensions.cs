using System;
using System.Linq;
using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert a string into JWS algorithm.
        /// </summary>
        /// <param name="alg">String to be converted</param>
        /// <returns>JWS algorithm</returns>
        public static JwsAlg ToJwsAlg(this string alg)
        {
            var algName = Enum.GetNames(typeof (JwsAlg))
                .SingleOrDefault(a => a.ToLowerInvariant() == alg.ToLowerInvariant());
            if (algName == null)
            {
                return JwsAlg.RS256;
            }

            return (JwsAlg)Enum.Parse(typeof (JwsAlg), algName);
        }
    }
}
