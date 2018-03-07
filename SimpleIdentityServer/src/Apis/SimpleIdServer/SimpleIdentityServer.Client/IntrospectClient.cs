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
using SimpleIdentityServer.Core.Common.DTOs;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    public interface IIntrospectClient
    {
        Task<Introspection> ExecuteAsync(string tokenUrl);
        Task<Introspection> ExecuteAsync(Uri tokenUri);
        Task<Introspection> ResolveAsync(string discoveryDocumentationUrl);
    }

    public enum TokenType
    {
        AccessToken,
        RefreshToken
    }

    internal class IntrospectClient : IIntrospectClient
    {
        private readonly RequestBuilder _requestBuilder;
        private readonly IIntrospectOperation _introspectOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public IntrospectClient(
            RequestBuilder requestBuilder,
            IIntrospectOperation introspectOperation,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _requestBuilder = requestBuilder;
            _introspectOperation = introspectOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public async Task<Introspection> ExecuteAsync(string tokenUrl)
        {
            if (string.IsNullOrWhiteSpace(tokenUrl))
            {
                throw new ArgumentNullException(nameof(tokenUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(tokenUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, tokenUrl));
            }

            return await ExecuteAsync(uri);
        }

        public async Task<Introspection> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }
            
            return await _introspectOperation.ExecuteAsync(_requestBuilder.Content,
                tokenUri,
                _requestBuilder.AuthorizationHeaderValue);
        }

        public async Task<Introspection> ResolveAsync(string discoveryDocumentationUrl)
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
            return await ExecuteAsync(discoveryDocument.IntrospectionEndPoint);
        }
    }
}
