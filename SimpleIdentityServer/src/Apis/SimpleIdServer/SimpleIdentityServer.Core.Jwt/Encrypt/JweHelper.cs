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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    public interface IJweHelper
    {
        IEncryption GetEncryptor(JweEnc enc);
    }

    public class JweHelper : IJweHelper
    {
        private readonly Dictionary<JweEnc, IEncryption> _mappingJweEncToKeySize;

        public JweHelper(IAesEncryptionHelper aesEncryptionHelper)
        {
            _mappingJweEncToKeySize = new Dictionary<JweEnc, IEncryption>
            {
                {
                    JweEnc.A128CBC_HS256, new AesEncryptionWithHmac(aesEncryptionHelper, 256)
                },
                {
                    JweEnc.A192CBC_HS384, new AesEncryptionWithHmac(aesEncryptionHelper, 384)
                },
                {
                    JweEnc.A256CBC_HS512, new AesEncryptionWithHmac(aesEncryptionHelper, 512)
                }
            };
        }

        public IEncryption GetEncryptor(JweEnc enc)
        {
            return _mappingJweEncToKeySize[enc];
        }
    }
}
