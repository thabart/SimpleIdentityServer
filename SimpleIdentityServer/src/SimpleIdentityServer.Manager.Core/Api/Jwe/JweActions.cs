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

using SimpleIdentityServer.Manager.Core.Api.Jwe.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Jwe
{
    public interface IJweActions
    {
        Task<JweInformationResult> GetJweInformation(GetJweParameter getJweParameter);
    }

    public class JweActions : IJweActions
    {
        private readonly IGetJweInformationAction _getJweInformationAction;

        #region Constructor

        public JweActions(IGetJweInformationAction getJweInformationAction)
        {
            _getJweInformationAction = getJweInformationAction;
        }

        #endregion

        #region Public methods

        public Task<JweInformationResult> GetJweInformation(GetJweParameter getJweParameter)
        {
            if (getJweParameter == null)
            {
                throw new ArgumentNullException(nameof(getJweParameter));
            }

            return _getJweInformationAction.ExecuteAsync(getJweParameter);
        }

        #endregion
    }
}
