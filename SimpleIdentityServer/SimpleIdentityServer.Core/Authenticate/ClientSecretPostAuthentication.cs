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
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientSecretPostAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);

        string GetClientId(AuthenticateInstruction instruction);
    }

    public class ClientSecretPostAuthentication : IClientSecretPostAuthentication
    {
        public Client AuthenticateClient(AuthenticateInstruction instruction, Client client)
        {
            var sameSecret = string.Compare(client.ClientSecret,
                        instruction.ClientSecretFromHttpRequestBody,
                        StringComparison.InvariantCultureIgnoreCase) == 0;
            return sameSecret ? client : null;
        }
        
        public string GetClientId(AuthenticateInstruction instruction)
        {
            return instruction.ClientIdFromHttpRequestBody;
        }
    }
}
