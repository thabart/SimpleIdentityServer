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

using SimpleIdentityServer.Manager.Core.Api.Jws.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Jws
{
    public interface IJwsActions
    {
        Task<JwsInformationResult> GetJwsInformation(GetJwsParameter getJwsParameter);

        Task<string> CreateJws(CreateJwsParameter createJwsParameter);
    }

    public class JwsActions : IJwsActions
    {
        private readonly IGetJwsInformationAction _getJwsInformationAction;

        private readonly ICreateJwsAction _createJwsAction;

        #region Constructor

        public JwsActions(
            IGetJwsInformationAction getJwsInformationAction, 
            ICreateJwsAction createJwsAction)
        {
            _getJwsInformationAction = getJwsInformationAction;
            _createJwsAction = createJwsAction;
        }

        #endregion
        
        #region Public methods

        public Task<JwsInformationResult> GetJwsInformation(GetJwsParameter getJwsParameter)
        {
            if (getJwsParameter == null || string.IsNullOrWhiteSpace(getJwsParameter.Jws))
            {
                throw new ArgumentNullException(nameof(getJwsParameter));
            }

            return _getJwsInformationAction.Execute(getJwsParameter);
        }

        public Task<string> CreateJws(CreateJwsParameter createJwsParameter)
        {
            if (createJwsParameter == null)
            {
                throw new ArgumentNullException(nameof(createJwsParameter));
            }

            return _createJwsAction.Execute(createJwsParameter);
        }

        #endregion
    }
}
