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
using SimpleIdentityServer.UmaManager.Client;

namespace SimpleIdentityServer.Proxy
{
    public interface ISecurityProxyFactory
    {
        ISecurityProxy GetProxy(SecurityOptions securityOptions);
    }

    public class SecurityProxyFactory : ISecurityProxyFactory
    {
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly IIdentityServerUmaManagerClientFactory _identityServerUmaManagerClientFactory;

        private readonly IIdentityServerUmaClientFactory _identityServerUmaClientFactory;

        #region Constructor

        public SecurityProxyFactory()
        {
            _identityServerClientFactory = new IdentityServerClientFactory();
            _identityServerUmaManagerClientFactory = new IdentityServerUmaManagerClientFactory();
            _identityServerUmaClientFactory = new IdentityServerUmaClientFactory();
        }

        #endregion

        #region Public methods

        public ISecurityProxy GetProxy(SecurityOptions securityOptions)
        {
            return new SecurityProxy(securityOptions,
                _identityServerClientFactory,
                _identityServerUmaManagerClientFactory,
                _identityServerUmaClientFactory);
        }

        #endregion
    }
}
