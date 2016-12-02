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
        ITokenGrantTypeSelector UseClientSecretJwtAuth(string jwt, string clientId);
        ITokenGrantTypeSelector UseClientPrivateKeyAuth(string jwt, string clientId);
        ITokenGrantTypeSelector UseNoAuthentication();
    }

    internal class ClientAuthSelector : IClientAuthSelector
    {
        private readonly ITokenClientFactory _tokenClientFactory;
        private readonly IIntrospectClientFactory _introspectClientFactory;

        public ClientAuthSelector(ITokenClientFactory tokenClientFactory, IIntrospectClientFactory introspectClientFactory)
        {
            _tokenClientFactory = tokenClientFactory;
            _introspectClientFactory = introspectClientFactory;
        }

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

            var requestBuilder = new RequestBuilder();
            var tokenClient = _tokenClientFactory.CreateClient(requestBuilder);
            var introspectClient = _introspectClientFactory.CreateClient(requestBuilder);
            var tokenGrantTypeSelector = new TokenGrantTypeSelector(requestBuilder, tokenClient, introspectClient);
            var authorizationValue = GetAuthorizationValue(clientId, clientSecret);
            requestBuilder.AuthorizationHeaderValue = authorizationValue;
            return tokenGrantTypeSelector;
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

            var requestBuilder = new RequestBuilder();
            var tokenClient = _tokenClientFactory.CreateClient(requestBuilder);
            var introspectClient = _introspectClientFactory.CreateClient(requestBuilder);
            var tokenGrantTypeSelector = new TokenGrantTypeSelector(requestBuilder, tokenClient, introspectClient);
            requestBuilder.SetClientCredentials(clientId, clientSecret);
            return tokenGrantTypeSelector;
        }

        public ITokenGrantTypeSelector UseClientSecretJwtAuth(string jwt, string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var requestBuilder = new RequestBuilder();
            var tokenClient = _tokenClientFactory.CreateClient(requestBuilder);
            var introspectClient = _introspectClientFactory.CreateClient(requestBuilder);
            var tokenGrantTypeSelector = new TokenGrantTypeSelector(requestBuilder, tokenClient, introspectClient);
            requestBuilder.SetClientAssertion(clientId, jwt, Core.Common.ClientAssertionTypes.JwtBearer);
            return tokenGrantTypeSelector;
        }

        public ITokenGrantTypeSelector UseClientPrivateKeyAuth(string jwt, string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var requestBuilder = new RequestBuilder();
            var tokenClient = _tokenClientFactory.CreateClient(requestBuilder);
            var introspectClient = _introspectClientFactory.CreateClient(requestBuilder);
            var tokenGrantTypeSelector = new TokenGrantTypeSelector(requestBuilder, tokenClient, introspectClient);
            requestBuilder.SetClientAssertion(clientId, jwt, Core.Common.ClientAssertionTypes.JwtBearer);
            return tokenGrantTypeSelector;
        }

        public ITokenGrantTypeSelector UseNoAuthentication()
        {
            var requestBuilder = new RequestBuilder();
            var tokenClient = _tokenClientFactory.CreateClient(requestBuilder);
            var introspectClient = _introspectClientFactory.CreateClient(requestBuilder);
            return new TokenGrantTypeSelector(requestBuilder, tokenClient, introspectClient);
        }

        private static string GetAuthorizationValue(string clientId, string clientSecret)
        {
            var concat = clientId + ":" + clientSecret;
            return concat.Base64Encode();
        }
    }
}
