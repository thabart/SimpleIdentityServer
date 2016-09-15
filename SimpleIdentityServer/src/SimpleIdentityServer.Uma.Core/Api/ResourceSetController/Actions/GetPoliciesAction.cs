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

using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    public interface IGetPoliciesAction
    {
        List<string> Execute(string resourceId);
    }

    internal class GetPoliciesAction : IGetPoliciesAction
    {
        #region Fields

        private readonly IPolicyRepository _repository;

        #endregion

        #region Constructor

        public GetPoliciesAction(IPolicyRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        #endregion
        public List<string> Execute(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            return _repository.GetPoliciesByResourceSetId(resourceId).Select(p => p.Id).ToList();
        }
    }
}
