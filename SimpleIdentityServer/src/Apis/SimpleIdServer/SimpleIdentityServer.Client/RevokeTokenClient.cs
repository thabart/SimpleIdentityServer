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

using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    public interface IRevokeTokenClient
    {
        Task<GetRevokeTokenResult> ExecuteAsync(string tokenUrl);
        Task<GetRevokeTokenResult> ExecuteAsync(Uri tokenUri);
        Task<GetRevokeTokenResult> ResolveAsync(string discoveryDocumentationUrl);
    }

    internal class RevokeTokenClient : IRevokeTokenClient
    {
        private readonly RequestBuilder _requestBuilder;
        private readonly IRevokeTokenOperation _revokeTokenOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public RevokeTokenClient(
            RequestBuilder requestBuilder,
            IRevokeTokenOperation revokeTokenOperation,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _requestBuilder = requestBuilder;
            _revokeTokenOperation = revokeTokenOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public async Task<GetRevokeTokenResult> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }

            return await _revokeTokenOperation.ExecuteAsync(_requestBuilder.Content, tokenUri, _requestBuilder.AuthorizationHeaderValue);
        }

        public async Task<GetRevokeTokenResult> ExecuteAsync(string revokeUrl)
        {
            if (string.IsNullOrWhiteSpace(revokeUrl))
            {
                throw new ArgumentNullException(nameof(revokeUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(revokeUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, revokeUrl));
            }

            return await ExecuteAsync(uri);
        }

        public async Task<GetRevokeTokenResult> ResolveAsync(string discoveryDocumentationUrl)
        {
            if (string.IsNullOrWhiteSpace(discoveryDocumentationUrl))
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(discoveryDocumentationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, discoveryDocumentationUrl));
            }

            var discoveryDocument = await _getDiscoveryOperation.ExecuteAsync(uri);
            return await ExecuteAsync(discoveryDocument.RevocationEndPoint);
        }
    }
}
