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

using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Selectors;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Proxy
{
    public interface IAuthenticationProvider
    {
        Task<string> GetIdentityToken(
            string login,
            string password,
            params string[] scopes);
    }

    internal class AuthenticationProvider : IAuthenticationProvider
    {
        private readonly AuthOptions _authOptions;

        private readonly IClientAuthSelector _clientAuthSelector;

        #region Constructor

        public AuthenticationProvider(
            AuthOptions options,
            IIdentityServerClientFactory identityServerClientFactory)
        {
            _authOptions = options;
            _clientAuthSelector = identityServerClientFactory.CreateTokenClient();
        }

        #endregion

        #region Public methods

        public async Task<string> GetIdentityToken(
            string login, 
            string password, 
            params string[] scopes)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var grantedToken = await _clientAuthSelector
                .UseClientSecretPostAuth(_authOptions.ClientId, _authOptions.ClientSecret)
                .UsePassword(login, password, scopes)
                .ResolveAsync(_authOptions.OpenIdConfigurationUrl);
            return grantedToken.IdToken;
        }

        #endregion
    }
}
