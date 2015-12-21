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
namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public class AesEncryption : IEncryption
    {
        public AesEncryptionResult Encrypt(
            string toEncrypt,
            JweAlg alg, 
            JweProtectedHeader protectedHeader,
            JsonWebKey jsonWebKey)
        {
            return null;
        }

        public AesEncryptionResult EncryptWithSymmetricPassword(
            string toEncrypt, 
            JweAlg alg, 
            JweProtectedHeader protectedHeader, 
            JsonWebKey jsonWebKey,
            string password)
        {
            return null;
        }
        
        public string Decrypt(
            string toDecrypt, 
            JweAlg alg, 
            JsonWebKey jsonWebKey)
        {
            return string.Empty;
        }

        public string DecryptWithSymmetricPassword(
            string toDecrypt, 
            JweAlg alg, 
            JsonWebKey jsonWebKey,
            string password)
        {
            return string.Empty;
        }
    }
}
