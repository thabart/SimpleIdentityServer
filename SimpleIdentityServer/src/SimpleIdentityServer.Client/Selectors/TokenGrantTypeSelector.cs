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
using SimpleIdentityServer.Client.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Client.Selectors
{
    public interface ITokenGrantTypeSelector
    {
        ITokenClient UseClientCredentials(params string[] scopes);
        ITokenClient UseClientCredentials(List<string> scopes);
        ITokenClient UsePassword(string userName, string password, params string[] scopes);
        ITokenClient UsePassword(string userName, string password, List<string> scopes);
        ITokenClient UseRefreshToken(string refreshToken);
    }

    internal class TokenGrantTypeSelector : ITokenGrantTypeSelector
    {
        private readonly ITokenRequestBuilder _tokenRequestBuilder;

        private readonly ITokenClient _tokenClient;

        #region Constructor

        public TokenGrantTypeSelector(
            ITokenRequestBuilder tokenRequestBuilder,
            ITokenClient tokenClient)
        {
            _tokenRequestBuilder = tokenRequestBuilder;
            _tokenClient = tokenClient;
        }

        #endregion

        #region Public methods

        public ITokenClient UseClientCredentials(params string[] scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            return UseClientCredentials(scopes.ToList());
        }

        public ITokenClient UseClientCredentials(List<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }
            
            _tokenRequestBuilder.TokenRequest.Scope = ConcatScopes(scopes);
            _tokenRequestBuilder.TokenRequest.GrantType = GrantTypeRequest.client_credentials;
            return _tokenClient;
        }

        public ITokenClient UsePassword(string userName, string password, params string[] scopes)
        {
            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            return UsePassword(userName, password, scopes.ToList());
        }

        public ITokenClient UsePassword(string userName, string password, List<string> scopes)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            _tokenRequestBuilder.TokenRequest.Username = userName;
            _tokenRequestBuilder.TokenRequest.Password = password;
            _tokenRequestBuilder.TokenRequest.Scope = ConcatScopes(scopes);
            _tokenRequestBuilder.TokenRequest.GrantType = GrantTypeRequest.password;
            return _tokenClient;
        }

        public ITokenClient UseRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            _tokenRequestBuilder.TokenRequest.RefreshToken = refreshToken;
            _tokenRequestBuilder.TokenRequest.GrantType = GrantTypeRequest.refresh_token;
            return _tokenClient;
        }

        #endregion

        #region Private static methods

        private static string ConcatScopes(List<string> scopes)
        {
            return string.Join(" ", scopes);
        }

        #endregion
    }
}
