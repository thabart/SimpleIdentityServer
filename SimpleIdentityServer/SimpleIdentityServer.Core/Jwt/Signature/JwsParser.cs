using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface IJwsParser
    {
        JwsPayload UnSigned(string jws);
    }

    public class JwsParser
    {
        public JwsPayload UnSigned(string jws)
        {
            return null;
        }

        /// <summary>
        /// Split the JWS into three parts.
        /// </summary>
        /// <param name="jws"></param>
        /// <returns></returns>
        private static List<string> GetParts(string jws)
        {
            var parts = jws.Split('.');
            if (parts == null || parts.Length < 3)
            {
                return new List<string>();
            }
        
            return parts.ToList();
        }
    }
}
