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

namespace SimpleIdentityServer.Core.Jwt.Converter
{
    public interface IJsonWebKeyConverter
    {
        IEnumerable<JsonWebKey> ConvertFromJson(string json);
    }

    public class JsonWebKeyConverter : IJsonWebKeyConverter
    {
        public IEnumerable<JsonWebKey> ConvertFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentNullException("json");
            }

            JsonWebKeySet jsonWebKeySet = null;
            try
            {
                jsonWebKeySet = json.DeserializeWithJavascript<JsonWebKeySet>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ErrorDescriptions.JwksCannotBeDeserialied, ex);
            }
            
            if (jsonWebKeySet == null ||
                    jsonWebKeySet.Keys == null ||
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
                        serializedKey = ExtractRsaKeyInformation(jsonWebKey,
                            useEnum);
                        break;
                    case Constants.KeyTypeValues.EcName:

                        break;
                }

                jsonWebKeyInformation.SerializedKey = serializedKey;
                result.Add(jsonWebKeyInformation);
            }

            return result;
        }

        private static string ExtractRsaKeyInformation(Dictionary<string, object> information,
            Use usage)
        {
            var result = string.Empty;
            if (usage == Use.Sig)
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

                using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
                {
                    rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                    return rsaCryptoServiceProvider.ToXmlString(false);
                }
            }

            return result;
        }

        private static string ExtractEcKeyInformation(Dictionary<string, object> information,
            Use usage)
        {
            return string.Empty;
        }
    }
}
