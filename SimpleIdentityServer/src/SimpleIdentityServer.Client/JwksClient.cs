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

using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Core.Common.DTOs;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    public interface IJwksClient
    {
        JsonWebKeySet Execute(string jwksUrl);

        JsonWebKeySet Execute(Uri jwksUri);

        Task<JsonWebKeySet> ExecuteAsync(string jwksUrl);

        Task<JsonWebKeySet> ExecuteAsync(Uri jwksUri);

        Task<JsonWebKeySet> ResolveAsync(string configurationUrl);
    }

    internal class JwksClient : IJwksClient
    {
        private readonly IGetJsonWebKeysOperation _getJsonWebKeysOperation;

        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        #region Constructor

        public JwksClient(
            IGetJsonWebKeysOperation getJsonWebKeysOperation,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _getJsonWebKeysOperation = getJsonWebKeysOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        #endregion

        #region Public methods

        public JsonWebKeySet Execute(string jwksUrl)
        {
            return ExecuteAsync(jwksUrl).Result;
        }

        public JsonWebKeySet Execute(Uri jwksUri)
        {
            return ExecuteAsync(jwksUri).Result;
        }

        public async Task<JsonWebKeySet> ExecuteAsync(string jwksUrl)
        {
            if (string.IsNullOrWhiteSpace(jwksUrl))
            {
                throw new ArgumentNullException(nameof(jwksUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(jwksUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, jwksUrl));
            }

            return await ExecuteAsync(uri);
        }

        public async Task<JsonWebKeySet> ExecuteAsync(Uri jwksUri)
        {
            if (jwksUri == null)
            {
                throw new ArgumentNullException(nameof(jwksUri));
            }

            return await _getJsonWebKeysOperation.ExecuteAsync(jwksUri);
        }

        public async Task<JsonWebKeySet> ResolveAsync(string configurationUrl)
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

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri);
            return await ExecuteAsync(discoveryDocument.JwksUri);
        }

        #endregion
    }
}
