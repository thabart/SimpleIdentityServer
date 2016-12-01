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

using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface IJwsParser
    {
        JwsPayload ValidateSignature(
            string jws,
            JsonWebKey jsonWebKey);

        JwsProtectedHeader GetHeader(string jws);

        JwsPayload GetPayload(string jws);
    }

    public class JwsParser : IJwsParser
    {
        private readonly ICreateJwsSignature _createJwsSignature;

        public JwsParser(
            ICreateJwsSignature createJwsSignature)
        {
            _createJwsSignature = createJwsSignature;
        }

        /// <summary>
        /// Validate the signature and returns the JWSPayLoad.
        /// </summary>
        /// <param name="jws"></param>
        /// <param name="jsonWebKey"></param>
        /// <returns></returns>
        public JwsPayload ValidateSignature(
            string jws,
            JsonWebKey jsonWebKey)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException("jws");
            }

            if (jsonWebKey == null)
            {
                throw new ArgumentNullException("jsonWebKey");
            }

            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }
            
            var base64EncodedProtectedHeader = parts[0];
            var base64EncodedSerialized = parts[1];
            var combinedProtectedHeaderAndPayLoad = string.Format("{0}.{1}", base64EncodedProtectedHeader,
                base64EncodedSerialized);
            
            var serializedProtectedHeader = base64EncodedProtectedHeader.Base64Decode();
            var serializedPayload = base64EncodedSerialized.Base64Decode();
            var signature = parts[2].Base64DecodeBytes();
            
            var protectedHeader = serializedProtectedHeader.DeserializeWithJavascript<JwsProtectedHeader>();
            
            JwsAlg jwsAlg;
            if (!Enum.TryParse(protectedHeader.Alg, out jwsAlg))
            {
                return null;
            }
            
            var signatureIsCorrect = false;
            switch (jsonWebKey.Kty)
            {
                case KeyType.RSA:
                    // To validate we need the parameters : modulus & exponent.
                    signatureIsCorrect = _createJwsSignature.VerifyWithRsa(
                        jwsAlg,
                        jsonWebKey.SerializedKey,
                        combinedProtectedHeaderAndPayLoad,
                        signature);
                    break;
#if NET46 || NET45
                case KeyType.EC:
                    signatureIsCorrect = _createJwsSignature.VerifyWithEllipticCurve(
                        jsonWebKey.SerializedKey,
                        combinedProtectedHeaderAndPayLoad,
                        signature);
                    break;
#endif
            }

            if (!signatureIsCorrect)
            {
                return null;
            }
            
            return serializedPayload.DeserializeWithJavascript<JwsPayload>();
        }

        public JwsProtectedHeader GetHeader(string jws)
        {
            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var base64EncodedProtectedHeader = parts[0];
            var serializedProtectedHeader = base64EncodedProtectedHeader.Base64Decode();
            return serializedProtectedHeader.DeserializeWithJavascript<JwsProtectedHeader>();
        }
        
        public JwsPayload GetPayload(string jws)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException("jws");
            }

            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var base64EncodedSerialized = parts[1];
            var serializedPayload = base64EncodedSerialized.Base64Decode();
            return serializedPayload.DeserializeWithJavascript<JwsPayload>();
        }

        /// <summary>
        /// Split the JWS into three parts.
        /// </summary>
        /// <param name="jws"></param>
        /// <returns></returns>
        private static List<string> GetParts(string jws)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                return null;
            }

            var parts = jws.Split('.');
            return parts.Length != 3 ? new List<string>() : parts.ToList();
        }
    }
}
