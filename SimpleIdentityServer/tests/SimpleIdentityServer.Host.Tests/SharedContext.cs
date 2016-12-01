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

using SimpleIdentityServer.Core.Jwt;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Host.Tests
{
    public class SharedContext
    {
        private static SharedContext _instance;

        private SharedContext()
        {
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = provider.ToXmlString(true);
            }

            SignatureKey = new JsonWebKey
            {
                Alg = AllAlg.RS256,
                KeyOps = new KeyOperations[]
                {
                    KeyOperations.Sign,
                    KeyOperations.Verify
                },
                Kid = "11",
                Kty = KeyType.RSA,
                Use = Use.Sig,
                SerializedKey = serializedRsa,
            };
            ModelSignatureKey = new DataAccess.SqlServer.Models.JsonWebKey
            {
                Alg = DataAccess.SqlServer.Models.AllAlg.RS256,
                KeyOps = "2,3",
                Kid = "11",
                Kty = DataAccess.SqlServer.Models.KeyType.RSA,
                Use = DataAccess.SqlServer.Models.Use.Sig,
                SerializedKey = serializedRsa,
            };
            EncryptionKey = new JsonWebKey
            {
                Alg = AllAlg.RSA1_5,
                KeyOps = new[]
                {
                    KeyOperations.Decrypt,
                    KeyOperations.Encrypt
                },
                Kid = "10",
                Kty = KeyType.RSA,
                Use = Use.Enc,
                SerializedKey = serializedRsa,
            };
            ModelEncryptionKey = new DataAccess.SqlServer.Models.JsonWebKey
            {
                Alg = DataAccess.SqlServer.Models.AllAlg.RSA1_5,
                KeyOps = "2,3",
                Kid = "10",
                Kty = DataAccess.SqlServer.Models.KeyType.RSA,
                Use = DataAccess.SqlServer.Models.Use.Enc,
                SerializedKey = serializedRsa,
            };
        }

        public static SharedContext Instance()
        {
            if (_instance == null)
            {
                _instance = new SharedContext();
            }

            return _instance;
        }

        public JsonWebKey EncryptionKey { get; }
        public DataAccess.SqlServer.Models.JsonWebKey ModelEncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
        public DataAccess.SqlServer.Models.JsonWebKey ModelSignatureKey { get; }
    }
}
