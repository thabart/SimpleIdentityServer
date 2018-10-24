#region copyright
// Copyright 2016 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS I S" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    public interface IRegistrationClient
    {
        GetRegisterClientResult Execute(Core.Common.DTOs.Requests.ClientRequest client, string jwksUrl, string accessToken);
        GetRegisterClientResult Execute(Core.Common.DTOs.Requests.ClientRequest client, Uri jwksUri, string accessToken);
        Task<GetRegisterClientResult> ExecuteAsync(Core.Common.DTOs.Requests.ClientRequest client, string registrationUrl, string accessToken);
        Task<GetRegisterClientResult> ExecuteAsync(Core.Common.DTOs.Requests.ClientRequest client, Uri registrationUri, string accessToken);
        Task<GetRegisterClientResult> ResolveAsync(Core.Common.DTOs.Requests.ClientRequest client, string configurationUrl, string accessToken);
    }

    internal class RegistrationClient : IRegistrationClient
    {
        private readonly IRegisterClientOperation _registerClientOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public RegistrationClient(IRegisterClientOperation registerClientOperation, IGetDiscoveryOperation getDiscoveryOperation)
        {
            _registerClientOperation = registerClientOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public GetRegisterClientResult Execute(Core.Common.DTOs.Requests.ClientRequest client, Uri registrationUri, string accessToken)
        {
            return ExecuteAsync(client, registrationUri, accessToken).Result;
        }

        public GetRegisterClientResult Execute(Core.Common.DTOs.Requests.ClientRequest client, string registrationUrl, string accessToken)
        {
            return ExecuteAsync(client, registrationUrl, accessToken).Result;
        }

        public Task<GetRegisterClientResult> ExecuteAsync(Core.Common.DTOs.Requests.ClientRequest client, Uri registrationUri, string accessToken)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (registrationUri == null)
            {
                throw new ArgumentNullException(nameof(registrationUri));
            }

            return _registerClientOperation.ExecuteAsync(client, registrationUri, accessToken);
        }

        public Task<GetRegisterClientResult> ExecuteAsync(Core.Common.DTOs.Requests.ClientRequest client, string registrationUrl, string accessToken)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(registrationUrl))
            {
                throw new ArgumentNullException(nameof(registrationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(registrationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, registrationUrl));
            }

            return ExecuteAsync(client, uri, accessToken);
        }

        public async Task<GetRegisterClientResult> ResolveAsync(Core.Common.DTOs.Requests.ClientRequest client, string configurationUrl, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, configurationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri).ConfigureAwait(false);
            return await ExecuteAsync(client, discoveryDocument.RegistrationEndPoint, accessToken).ConfigureAwait(false);
        }
    }
}
