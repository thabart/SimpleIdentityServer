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
using SimpleIdentityServer.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IClientValidator
    {
        IEnumerable<string> GetRedirectionUrls(Core.Common.Models.Client client, params string[] urls);
        bool CheckGrantTypes(Core.Common.Models.Client client, params GrantType[] grantTypes);
        bool CheckResponseTypes(Core.Common.Models.Client client, params ResponseType[] responseTypes);
        bool CheckPkce(Core.Common.Models.Client client, string codeVerifier, AuthorizationCode code);
    }

    public class ClientValidator : IClientValidator
    {        
        public IEnumerable<string> GetRedirectionUrls(Core.Common.Models.Client client, params string[] urls)
        {
            if (urls == null ||
                client == null || 
                client.RedirectionUrls == null || 
                !client.RedirectionUrls.Any())
            {
                return new string[0];
            }

            return client.RedirectionUrls.Where(r => urls.Contains(r));
        }

        public bool CheckGrantTypes(Core.Common.Models.Client client, params GrantType[] grantTypes)
        {
            if (client == null || grantTypes == null)
            {
                return false;
            }

            if (client.GrantTypes == null || !client.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }

            return client.GrantTypes != null && grantTypes.All(gt => client.GrantTypes.Contains(gt));
        }
        
        public bool CheckResponseTypes(Core.Common.Models.Client client, params ResponseType[] responseTypes)
        {
            if (client == null)
            {
                return false;
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Any())
            {
                client.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }

            return client.ResponseTypes != null && responseTypes.All(rt => client.ResponseTypes.Contains(rt));
        }

        public bool CheckPkce(Client client, string codeVerifier, AuthorizationCode code)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (!client.RequirePkce)
            {
                return true;
            }

            if (code.CodeChallengeMethod.Value == CodeChallengeMethods.Plain)
            {
                return codeVerifier == code.CodeChallenge;
            }

            var hashed = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            var codeChallenge = hashed.Base64EncodeBytes();
            return code.CodeChallenge == codeChallenge;
        }
    }
}
