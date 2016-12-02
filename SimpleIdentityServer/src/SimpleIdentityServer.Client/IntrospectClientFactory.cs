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
using SimpleIdentityServer.Client.Operations;

namespace SimpleIdentityServer.Client
{
    public interface IIntrospectClientFactory
    {
        IIntrospectClient CreateClient(RequestBuilder requestBuilder);
    }

    internal class IntrospectClientFactory : IIntrospectClientFactory
    {
        private readonly IIntrospectOperation _introspectOperation;
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public IntrospectClientFactory(IIntrospectOperation introspectOperation, IGetDiscoveryOperation getDiscoveryOperation)
        {
            _introspectOperation = introspectOperation;
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        public IIntrospectClient CreateClient(RequestBuilder requestBuilder)
        {
            return new IntrospectClient(requestBuilder, _introspectOperation, _getDiscoveryOperation);
        }
    }
}
