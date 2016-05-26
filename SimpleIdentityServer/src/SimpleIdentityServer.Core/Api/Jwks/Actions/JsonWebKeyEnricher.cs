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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    public interface IJsonWebKeyEnricher
    {
        Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey);

        Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey);
    }

    public class JsonWebKeyEnricher : IJsonWebKeyEnricher
    {
        private readonly Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>> _mappingKeyTypeAndPublicKeyEnricher;

        public JsonWebKeyEnricher()
        {
            _mappingKeyTypeAndPublicKeyEnricher = new Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>>
            {
                {
                    KeyType.RSA, SetRsaPublicKeyInformation
                }
            };
        }
        
        public Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey)
        {
            var result = new Dictionary<string, object>();
            var enricher = _mappingKeyTypeAndPublicKeyEnricher[jsonWebKey.Kty];
            enricher(result, jsonWebKey);
            return result;
        }

        public Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey)
        {
            return new Dictionary<string, object>
            {
                {
                    Jwt.Constants.JsonWebKeyParameterNames.KeyTypeName, Jwt.Constants.MappingKeyTypeEnumToName[jsonWebKey.Kty]
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.UseName, Jwt.Constants.MappingUseEnumerationToName[jsonWebKey.Use]
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.AlgorithmName, Jwt.Constants.MappingNameToAllAlgEnum.SingleOrDefault(kp => kp.Value == jsonWebKey.Alg).Key
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.KeyIdentifierName, jsonWebKey.Kid
                }
                // TODO : we still need to support the other parameters x5u & x5c & x5t & x5t#S256
            };
        }

        public void SetRsaPublicKeyInformation(Dictionary<string, object> result, JsonWebKey jsonWebKey)
        {
#if NET46
            using (var provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(jsonWebKey.SerializedKey);
                var rsaParameters = provider.ExportParameters(false);
                // Export the modulus
                var modulus = rsaParameters.Modulus.Base64EncodeBytes();
                // Export the exponent
                var exponent = rsaParameters.Exponent.Base64EncodeBytes();

                result.Add(Jwt.Constants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
                result.Add(Jwt.Constants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
            }
#else
            using (var provider = new RSAOpenSsl())
            {
                provider.FromXmlString(jsonWebKey.SerializedKey);
                var rsaParameters = provider.ExportParameters(false);
                // Export the modulus
                var modulus = rsaParameters.Modulus.Base64EncodeBytes();
                // Export the exponent
                var exponent = rsaParameters.Exponent.Base64EncodeBytes();

                result.Add(Jwt.Constants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
                result.Add(Jwt.Constants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
            }
#endif
        }
    }
}
