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

using System.Diagnostics.Contracts;

namespace System.Security.Cryptography
{
    public static class ECDsaCngExtensions
    {
        #region Public static methods

        public static byte[] SignData(
            this ECDsaCng ecdsaCng, 
            byte[] data, 
            int offset,
            int count)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || offset > data.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > data.Length - offset)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            
            using (var hashAlgorithm = new BCryptHashAlgorithm(CngAlgorithm.Sha256, BCryptNative.ProviderName.MicrosoftPrimitiveProvider))
            {
                hashAlgorithm.HashCore(data, offset, count);
                byte[] hashValue = hashAlgorithm.HashFinal();
                return ecdsaCng.SignHash(hashValue);
            }
        }

        public static bool VerifyData(
            this ECDsaCng ecdsaCng, 
            byte[] data,
            int offset, 
            int count,
            byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || offset > data.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > data.Length - offset)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            using (BCryptHashAlgorithm hashAlgorithm = new BCryptHashAlgorithm(CngAlgorithm.Sha256, BCryptNative.ProviderName.MicrosoftPrimitiveProvider))
            {
                hashAlgorithm.HashCore(data, offset, count);
                byte[] hashValue = hashAlgorithm.HashFinal();

                return ecdsaCng.VerifyHash(hashValue, signature);
            }
        }

        #endregion
    }
}
