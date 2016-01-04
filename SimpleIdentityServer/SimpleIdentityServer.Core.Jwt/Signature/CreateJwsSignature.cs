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

using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface ICreateJwsSignature
    {
        string SignWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string combinedJwsNotSigned);

        bool VerifyWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string input,
            byte[] signature);

        string SignWithEllipseCurve(string serializedKeys,
           string combinedJwsNotSigned);

         bool VerifyWithEllipticCurve(string serializedKeys,
            string message,
            byte[] signature);
    }

    public class CreateJwsSignature : ICreateJwsSignature
    {
        private readonly Dictionary<JwsAlg, string> _mappingJwsAlgorithmToRsaHashingAlgorithms = new Dictionary<JwsAlg, string>
        {
            {
                JwsAlg.RS256, "SHA256"
            },
            {
                JwsAlg.RS384, "SHA384"
            },
            {
                JwsAlg.RS512, "SHA512"
            }
        };

        private readonly ICngKeySerializer _cngKeySerializer;

        public CreateJwsSignature(ICngKeySerializer cngKeySerializer)
        {
            _cngKeySerializer = cngKeySerializer;
        }

        #region Rsa agorithm

        public string SignWithRsa(
            JwsAlg algorithm, 
            string serializedKeys,
            string combinedJwsNotSigned)
        {
            if (!_mappingJwsAlgorithmToRsaHashingAlgorithms.ContainsKey(algorithm))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            var hashMethod = _mappingJwsAlgorithmToRsaHashingAlgorithms[algorithm];
            using (var rsa = new RSACryptoServiceProvider())
            {
                var bytesToBeSigned = ASCIIEncoding.ASCII.GetBytes(combinedJwsNotSigned);
                rsa.FromXmlString(serializedKeys);
                var byteToBeConverted = rsa.SignData(bytesToBeSigned, hashMethod);
                return byteToBeConverted.Base64EncodeBytes();
            }
        }

        public bool VerifyWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string input,
            byte[] signature)
        {
            if (!_mappingJwsAlgorithmToRsaHashingAlgorithms.ContainsKey(algorithm))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            var plainBytes = ASCIIEncoding.ASCII.GetBytes(input);
            var hashMethod = _mappingJwsAlgorithmToRsaHashingAlgorithms[algorithm];
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(serializedKeys);
                return rsa.VerifyData(plainBytes, hashMethod, signature);
            }
        }

        #endregion

        #region Elliptic Curve algorithm

        public string SignWithEllipseCurve(string serializedKeys,
            string combinedJwsNotSigned)
        {
            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            if (string.IsNullOrWhiteSpace(combinedJwsNotSigned))
            {
                throw new ArgumentNullException("combinedJwsNotSigned");
            }

            var cngKey = _cngKeySerializer.Deserialize(serializedKeys);
            var plainTextBytes = Encoding.UTF8.GetBytes(combinedJwsNotSigned);
            using (var ec = new ECDsaCng(cngKey))
            {
                return ec.SignData(plainTextBytes).Base64EncodeBytes();
            }
        }

        public bool VerifyWithEllipticCurve(string serializedKeys,
            string message,
            byte[] signature)
        {
            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("message");
            }

            if (signature == null || !signature.Any())
            {
                throw new ArgumentNullException("signature");
            }

            var cngKey = _cngKeySerializer.Deserialize(serializedKeys);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using (var ecsdaSignature = new ECDsaCng(cngKey))
            {
                return ecsdaSignature.VerifyData(messageBytes,
                    signature);
            }
        }

        #endregion
    }
}
