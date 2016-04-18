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
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.Errors;
using SimpleIdentityServer.Client.Operations;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client
{
    public interface ITokenClient
    {
        Task<GrantedToken> ExecuteAsync(string tokenUrl);

        Task<GrantedToken> ExecuteAsync(Uri tokenUri);

        Task<GrantedToken> ResolveAsync(string discoveryDocumentationUrl);
    }

    internal class TokenClient : ITokenClient
    {
        private readonly IPostTokenOperation _postTokenOperation;

        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        private readonly ITokenRequestBuilder _tokenRequestBuilder;

        #region Constructor

        public TokenClient(
            ITokenRequestBuilder tokenRequestBuilder,
            IPostTokenOperation postTokenOperation,
            IGetDiscoveryOperation getDiscoveryOperation)
        {
            _tokenRequestBuilder = tokenRequestBuilder;
            _postTokenOperation = postTokenOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        #endregion

        #region Public methods
        
        public async Task<GrantedToken> ExecuteAsync(string tokenUrl)
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

        public async Task<GrantedToken> ExecuteAsync(Uri tokenUri)
        {
            if (tokenUri == null)
            {
                throw new ArgumentNullException(nameof(tokenUri));
            }

            var tokenRequest = _tokenRequestBuilder.TokenRequest;
            var authorizationHeaderValue = _tokenRequestBuilder.AuthorizationHeaderValue;
            return await _postTokenOperation.ExecuteAsync(tokenRequest,
                tokenUri,
                authorizationHeaderValue);
        }

        public async Task<GrantedToken> ResolveAsync(string discoveryDocumentationUrl)
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
            return await ExecuteAsync(discoveryDocument.TokenEndPoint);
        }

        #endregion
    }
}
