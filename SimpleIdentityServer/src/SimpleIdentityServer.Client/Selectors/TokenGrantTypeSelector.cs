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
        ITokenClient UseClientCredentials(string scope);

        ITokenClient UseClientCredentials(params string[] scopes);

        ITokenClient UseClientCredentials(List<string> scopes);
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

        public ITokenClient UseClientCredentials(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            _tokenRequestBuilder.TokenRequest.Scope = scope;
            _tokenRequestBuilder.TokenRequest.GrantType = GrantTypeRequest.client_credentials;
            return _tokenClient;
        }

        public ITokenClient UseClientCredentials(params string[] scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            return UseClientCredentials(ConcatScopes(scopes.ToList()));
        }

        public ITokenClient UseClientCredentials(List<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            return UseClientCredentials(ConcatScopes(scopes));
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
