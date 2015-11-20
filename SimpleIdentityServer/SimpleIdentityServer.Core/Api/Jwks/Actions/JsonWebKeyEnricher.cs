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
                    Jwt.Constants.JsonWebKeyParameterNames.KeyIdentifierName, jsonWebKey.Kid
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.UseName, Jwt.Constants.MappingUseEnumerationToName[jsonWebKey.Use]
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.KeyOperationsName, jsonWebKey.KeyOps.Select(j => Jwt.Constants.MappingKeyOperationToName[j]).ToArray()
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.Algorithm, Jwt.Constants.JwsAlgNames.RS256
                },
                {
                    Jwt.Constants.JsonWebKeyParameterNames.KeyTypeName, Jwt.Constants.MappingKeyTypeEnumToName[jsonWebKey.Kty]
                }
            };
        }

        public void SetRsaPublicKeyInformation(Dictionary<string, object> result, JsonWebKey jsonWebKey)
        {
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
        } 
    }
}
