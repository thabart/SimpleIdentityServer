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

using System;
using System.Threading.Tasks;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Errors;

namespace SimpleIdentityServer.Client
{
    public interface IRegistrationClient
    {
        Core.Common.DTOs.ClientRegistrationResponse Execute(Core.Common.DTOs.Client client, string jwksUrl);
        Core.Common.DTOs.ClientRegistrationResponse Execute(Core.Common.DTOs.Client client, Uri jwksUri);
        Task<Core.Common.DTOs.ClientRegistrationResponse> ExecuteAsync(Core.Common.DTOs.Client client, string registrationUrl);
        Task<Core.Common.DTOs.ClientRegistrationResponse> ExecuteAsync(Core.Common.DTOs.Client client, Uri registrationUri);
        Task<Core.Common.DTOs.ClientRegistrationResponse> ResolveAsync(Core.Common.DTOs.Client client, string configurationUrl);
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

        public Core.Common.DTOs.ClientRegistrationResponse Execute(Core.Common.DTOs.Client client, Uri registrationUri)
        {
            return ExecuteAsync(client, registrationUri).Result;
        }

        public Core.Common.DTOs.ClientRegistrationResponse Execute(Core.Common.DTOs.Client client, string registrationUrl)
        {
            return ExecuteAsync(client, registrationUrl).Result;
        }

        public async Task<Core.Common.DTOs.ClientRegistrationResponse> ExecuteAsync(Core.Common.DTOs.Client client, Uri registrationUri)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (registrationUri == null)
            {
                throw new ArgumentNullException(nameof(registrationUri));
            }

            return await _registerClientOperation.ExecuteAsync(client, registrationUri, string.Empty).ConfigureAwait(false);
        }

        public async Task<Core.Common.DTOs.ClientRegistrationResponse> ExecuteAsync(Core.Common.DTOs.Client client, string registrationUrl)
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

            return await ExecuteAsync(client, uri).ConfigureAwait(false);
        }

        public async Task<Core.Common.DTOs.ClientRegistrationResponse> ResolveAsync(Core.Common.DTOs.Client client, string configurationUrl)
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
            return await ExecuteAsync(client, discoveryDocument.RegistrationEndPoint).ConfigureAwait(false);
        }
    }
}
