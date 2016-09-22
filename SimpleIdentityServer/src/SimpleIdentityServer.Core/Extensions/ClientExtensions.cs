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
using System.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class ClientExtensions
    {
        #region Public static methods
        
        public static JwsAlg? GetIdTokenSignedResponseAlg(this Models.Client client)
        {
            var algName = client.IdTokenSignedResponseAlg;
            return GetDefaultJwsAlg(algName);
        }

        public static JweAlg? GetIdTokenEncryptedResponseAlg(this Models.Client client)
        {
            var algName = client.IdTokenEncryptedResponseAlg;
            return GetDefaultEncryptAlg(algName);
        }

        public static JweEnc? GetIdTokenEncryptedResponseEnc(this Models.Client client)
        {
            var encName = client.IdTokenEncryptedResponseEnc;
            return GetDefaultEncryptEnc(encName);
        }

        public static JwsAlg? GetUserInfoSignedResponseAlg(this Models.Client client)
        {
            var algName = client.UserInfoSignedResponseAlg;
            return GetDefaultJwsAlg(algName);
        }

        public static JweAlg? GetUserInfoEncryptedResponseAlg(this Models.Client client)
        {
            var algName = client.UserInfoEncryptedResponseAlg;
            return GetDefaultEncryptAlg(algName);
        }

        public static JweEnc? GetUserInfoEncryptedResponseEnc(this Models.Client client)
        {
            var encName = client.UserInfoEncryptedResponseEnc;
            return GetDefaultEncryptEnc(encName);
        }

        public static JwsAlg? GetRequestObjectSigningAlg(this Models.Client client)
        {
            var algName = client.RequestObjectSigningAlg;
            return GetDefaultJwsAlg(algName);
        }
        
        public static JweAlg? GetRequestObjectEncryptionAlg(this Models.Client client)
        {
            var algName = client.RequestObjectEncryptionAlg;
            return GetDefaultEncryptAlg(algName);
        }

        public static JweEnc? GetRequestObjectEncryptionEnc(this Models.Client client)
        {
            var encName = client.RequestObjectEncryptionEnc;
            return GetDefaultEncryptEnc(encName);
        }

        #endregion

        #region Private static methods

        private static JweAlg? GetDefaultEncryptAlg(string algName)
        {
            JweAlg? algEnum = null;
            if (!string.IsNullOrWhiteSpace(algName) &&
                Jwt.Constants.MappingNameToJweAlgEnum.Keys.Contains(algName))
            {
                algEnum = Jwt.Constants.MappingNameToJweAlgEnum[algName];
            }

            return algEnum;
        }

        private static JweEnc? GetDefaultEncryptEnc(string encName)
        {
            JweEnc? encEnum = null;
            if (!string.IsNullOrWhiteSpace(encName) &&
                Jwt.Constants.MappingNameToJweEncEnum.Keys.Contains(encName))
            {
                encEnum = Jwt.Constants.MappingNameToJweEncEnum[encName];
            }

            return encEnum;
        }

        private static JwsAlg? GetDefaultJwsAlg(string algName)
        {
            JwsAlg? signedAlgorithm = null;
            JwsAlg result;
            if (!string.IsNullOrWhiteSpace(algName)
                && Enum.TryParse(algName, true, out result))
            {
                signedAlgorithm = result;
            }

            return signedAlgorithm;
        }

        #endregion
    }
}
