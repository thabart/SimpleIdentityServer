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
using SimpleIdentityServer.Core.Jwt.Exceptions;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Jwt.Serializer;
using System.Xml.Serialization;
using System.IO;

namespace SimpleIdentityServer.Core.Jwt.Converter
{
    public interface IJsonWebKeyConverter
    {
        IEnumerable<JsonWebKey> ExtractSerializedKeys(JsonWebKeySet jsonWebKeySet);
    }

    public class JsonWebKeyConverter : IJsonWebKeyConverter
    {
        public IEnumerable<JsonWebKey> ExtractSerializedKeys(JsonWebKeySet jsonWebKeySet)
        {
            if (jsonWebKeySet == null)
            {
                throw new ArgumentNullException("jsonWebKeySet");
            }
            
            if (jsonWebKeySet.Keys == null ||
                    !jsonWebKeySet.Keys.Any())
            {
                return new List<JsonWebKey>();
            }

            var result = new List<JsonWebKey>();
            foreach (var jsonWebKey in jsonWebKeySet.Keys)
            {
                var keyType = jsonWebKey.FirstOrDefault(j => j.Key == Constants.JsonWebKeyParameterNames.KeyTypeName);
                var use = jsonWebKey.FirstOrDefault(j => j.Key == Constants.JsonWebKeyParameterNames.UseName);
                var kid =
                    jsonWebKey.FirstOrDefault(j => j.Key == Constants.JsonWebKeyParameterNames.KeyIdentifierName);
                if (keyType.Equals(default(KeyValuePair<string, object>)) ||
                    use.Equals(default(KeyValuePair<string, object>)) ||
                    kid.Equals(default(KeyValuePair<string, object>)) ||
                    !Constants.MappingKeyTypeEnumToName.Values.Contains(keyType.Value) ||
                    !Constants.MappingUseEnumerationToName.Values.Contains(use.Value))
                {
                    throw new InvalidOperationException(ErrorDescriptions.JwkIsInvalid);
                }

                var useEnum = Constants.MappingUseEnumerationToName
                    .FirstOrDefault(m => m.Value == use.Value.ToString()).Key;
                var keyTypeEnum = Constants.MappingKeyTypeEnumToName
                    .FirstOrDefault(k => k.Value == keyType.Value.ToString()).Key;

                var jsonWebKeyInformation = new JsonWebKey
                {
                    Use = useEnum,
                    Kid = kid.Value.ToString(),
                    Kty = keyTypeEnum
                };
                jsonWebKeyInformation.Use = useEnum;


                var serializedKey = string.Empty;
                switch (keyType.Value.ToString())
                {
                    case Constants.KeyTypeValues.RsaName:
                        serializedKey = ExtractRsaKeyInformation(jsonWebKey);
                        break;
                    case Constants.KeyTypeValues.EcName:
                        serializedKey = ExtractEcKeyInformation(jsonWebKey);
                        break;
                }

                jsonWebKeyInformation.SerializedKey = serializedKey;
                result.Add(jsonWebKeyInformation);
            }

            return result;
        }

        private static string ExtractRsaKeyInformation(Dictionary<string, object> information)
        {
            var modulusKeyPair = information.FirstOrDefault(i => i.Key == Constants.JsonWebKeyParameterNames.RsaKey.ModulusName);
            var exponentKeyPair = information.FirstOrDefault(i => i.Key == Constants.JsonWebKeyParameterNames.RsaKey.ExponentName);
            if (modulusKeyPair.Equals(default(KeyValuePair<string, object>)) ||
                exponentKeyPair.Equals(default(KeyValuePair<string, object>)))
            {
                throw new InvalidOperationException(ErrorDescriptions.CannotExtractParametersFromJsonWebKey);
            }
            
            var rsaParameters = new RSAParameters
            {
                Modulus = modulusKeyPair.Value.ToString().Base64DecodeBytes(),
                Exponent = exponentKeyPair.Value.ToString().Base64DecodeBytes()
            };

#if NET46 || NET45
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            {
                rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                return rsaCryptoServiceProvider.ToXmlString(false);
            }
#else
            using (var rsaCryptoServiceProvider = new RSAOpenSsl())
            {
                rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                return rsaCryptoServiceProvider.ToXmlString(false);
            }
#endif
        }

        private string ExtractEcKeyInformation(Dictionary<string, object> information)
        {
            var xCoordinate = information.FirstOrDefault(i => i.Key == Constants.JsonWebKeyParameterNames.EcKey.XCoordinateName);
            var yCoordinate = information.FirstOrDefault(i => i.Key == Constants.JsonWebKeyParameterNames.EcKey.YCoordinateName);
            if (xCoordinate.Equals(default(KeyValuePair<string, object>)) ||
                yCoordinate.Equals(default(KeyValuePair<string, object>)))
            {
                throw new InvalidOperationException(ErrorDescriptions.CannotExtractParametersFromJsonWebKey);
            }
            
            byte[] xCoordinateBytes,
                yCoordinateBytes;
            try
            {
                xCoordinateBytes = xCoordinate.Value.ToString().Base64DecodeBytes();
                yCoordinateBytes = yCoordinate.Value.ToString().Base64DecodeBytes();
            }
            catch (Exception)
            {
                throw new InvalidOperationException(ErrorDescriptions.OneOfTheParameterIsNotBase64Encoded);
            }
            
            var cngKeySerialized = new CngKeySerialized
            {
                X = xCoordinateBytes,
                Y = yCoordinateBytes
            };
            
            var serializer = new XmlSerializer(typeof(CngKeySerialized));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, cngKeySerialized);
                return writer.ToString();
            }
        }
    }
}
