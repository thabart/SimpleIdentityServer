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

namespace SimpleIdentityServer.Proxy
{
    public interface IAuthenticationProviderFactory
    {
        IAuthenticationProvider GetAuthProvider(AuthOptions options);
    }

    public class AuthenticationProviderFactory : IAuthenticationProviderFactory
    {
        #region Fields

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        #endregion

        #region Constructor

        public AuthenticationProviderFactory()
        {
            _identityServerClientFactory = new IdentityServerClientFactory();
        }

        #endregion

        #region Public methods

        public IAuthenticationProvider GetAuthProvider(AuthOptions options)
        {
            return new AuthenticationProvider(options, _identityServerClientFactory);
        }

        #endregion
    }
}
