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

using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.IntrospectionController.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.IntrospectionController
{
    public interface IIntrospectionActions
    {
        Task<IntrospectionResponse> GetIntrospection(string rpt);
        Task<IEnumerable<IntrospectionResponse>> GetIntrospection(IEnumerable<string> rpts);
    }

    internal class IntrospectionActions : IIntrospectionActions
    {
        private readonly IGetIntrospectAction _getIntrospectAction;

        public IntrospectionActions(IGetIntrospectAction getIntrospectAction)
        {
            _getIntrospectAction = getIntrospectAction;
        }

        public Task<IntrospectionResponse> GetIntrospection(string rpt)
        {
            return _getIntrospectAction.Execute(rpt);
        }

        public Task<IEnumerable<IntrospectionResponse>> GetIntrospection(IEnumerable<string> rpts)
        {
            return _getIntrospectAction.Execute(rpts);
        }
    }
}
