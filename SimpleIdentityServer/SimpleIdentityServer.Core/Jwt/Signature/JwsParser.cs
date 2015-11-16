using SimpleIdentityServer.Core.Extensions;
using System;
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
            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var serializedProtectedHeader = parts[0].Base64Decode();
            var serializedPayload = parts[1].Base64Decode();
            var signature = parts[2].Base64Decode();

            var protectedHeader = serializedProtectedHeader.DeserializeWithJavascript<JwsProtectedHeader>();
            var payload = serializedPayload.DeserializeWithJavascript<JwsPayload>();
            
            JwsAlg jwsAlg;
            if (!Enum.TryParse(protectedHeader.alg, out jwsAlg))
            {
                // TODO : maybe throw an exception.
                return null;
            }

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
