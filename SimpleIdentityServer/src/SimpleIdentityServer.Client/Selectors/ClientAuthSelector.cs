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
using SimpleIdentityServer.Core.Common.Extensions;
using System;

namespace SimpleIdentityServer.Client.Selectors
{
    public interface IClientAuthSelector
    {
        ITokenGrantTypeSelector UseClientSecretBasicAuth(string clientId, string clientSecret);

        ITokenGrantTypeSelector UseClientSecretPostAuth(string clientId, string clientSecret);
    }

    internal class ClientAuthSelector : IClientAuthSelector
    {
        private readonly ITokenRequestBuilder _tokenRequestBuilder;

        private readonly ITokenGrantTypeSelector _tokenGrantTypeSelector;

        #region Constructor

        public ClientAuthSelector(
            ITokenRequestBuilder tokenRequestBuilder,
            ITokenGrantTypeSelector tokenGrantTypeSelector)
        {
            _tokenRequestBuilder = tokenRequestBuilder;
            _tokenGrantTypeSelector = tokenGrantTypeSelector;
        }

        #endregion

        #region Public methods

        public ITokenGrantTypeSelector UseClientSecretBasicAuth(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            var authorizationValue = GetAuthorizationValue(clientId, clientSecret);
            _tokenRequestBuilder.AuthorizationHeaderValue = authorizationValue;
            return _tokenGrantTypeSelector;
        }

        public ITokenGrantTypeSelector UseClientSecretPostAuth(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            _tokenRequestBuilder.TokenRequest.ClientId = clientId;
            _tokenRequestBuilder.TokenRequest.ClientSecret = clientSecret;
            return _tokenGrantTypeSelector;
        }

        public ITokenGrantTypeSelector UseClientSecretJwtAuth(string token)
        {
            return null;
        }

        public ITokenGrantTypeSelector UseClientPrivateKeyAuth(string token)
        {
            return null;
        }

        #endregion

        #region Private static methods

        private static string GetAuthorizationValue(string clientId, string clientSecret)
        {
            var concat = clientId + ":" + clientSecret;
            return concat.Base64Encode();
        }

        #endregion
    }
}
