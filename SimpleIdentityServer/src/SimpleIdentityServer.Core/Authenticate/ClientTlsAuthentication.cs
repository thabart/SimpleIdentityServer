#region copyright
// Copyright 2017 Habart Thierry
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

namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientTlsAuthentication
    {
        Models.Client AuthenticateClient(AuthenticateInstruction instruction, Models.Client client);
    }

    internal class ClientTlsAuthentication : IClientTlsAuthentication
    {
        public Models.Client AuthenticateClient(AuthenticateInstruction instruction, Models.Client client)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (instruction.Certificate == null || client.Secrets == null)
            {
                return null;
            }

            var thumbPrint = client.Secrets.FirstOrDefault(s => s.Type == Models.ClientSecretTypes.X509Thumbprint);
            var name = client.Secrets.FirstOrDefault(s => s.Type == Models.ClientSecretTypes.X509Name);
            if (thumbPrint == null || name == null)
            {
                return null;
            }

            var certificate = instruction.Certificate;
            var isSameThumbPrint = thumbPrint == null || thumbPrint != null && thumbPrint.Value == certificate.Thumbprint;
            var isSameName = name == null || name != null && name.Value == certificate.SubjectName.Name;
            return isSameName && isSameThumbPrint ? client : null;
        }
    }
}
