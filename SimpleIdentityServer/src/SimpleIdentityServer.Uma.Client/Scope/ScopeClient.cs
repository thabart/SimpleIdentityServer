#region copyright
// Copyright 2016 Habart Thierry
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

using SimpleIdentityServer.Client.Configuration;

namespace SimpleIdentityServer.Client.Scope
{
    public interface IScopeClient
    {

    }

    internal class ScopeClient : IScopeClient
    {
        private readonly IGetScopeOperation _getScopeOperation;
        private readonly IGetScopesOperation _getScopesOperation;
        private readonly IDeleteScopeOperation _deleteScopeOperation;
        private readonly IAddScopeOperation _addScopeOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public ScopeClient(IGetScopeOperation getScopeOperation,
            IGetScopesOperation getScopesOperation,
            IDeleteScopeOperation deleteScopeOperation,
            IUpdateScopeOperation updateScopeOperation,
            IAddScopeOperation addScopeOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _getScopeOperation = getScopeOperation;
            _getScopesOperation = getScopesOperation;
            _deleteScopeOperation = deleteScopeOperation;
            _addScopeOperation = addScopeOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }
    }
}
