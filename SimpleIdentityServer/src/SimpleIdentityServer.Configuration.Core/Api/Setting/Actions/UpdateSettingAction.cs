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

using SimpleIdentityServer.Configuration.Core.Parameters;
using SimpleIdentityServer.Configuration.Core.Repositories;
using System;

namespace SimpleIdentityServer.Configuration.Core.Api.Setting.Actions
{
    public interface IUpdateSettingAction
    {
        bool Execute(UpdateSettingParameter updateSettingParameter);
    }

    internal class UpdateSettingAction : IUpdateSettingAction
    {
        #region Fields

        private readonly ISettingRepository _settingRepository;

        #endregion

        #region Constructor

        public UpdateSettingAction(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        #endregion

        #region Public methods

        public bool Execute(UpdateSettingParameter updateSettingParameter)
        {
            if (updateSettingParameter == null)
            {
                throw new ArgumentNullException(nameof(updateSettingParameter));
            }

            if (string.IsNullOrWhiteSpace(updateSettingParameter.Key))
            {
                throw new ArgumentNullException(nameof(updateSettingParameter.Key));
            }

            return _settingRepository.Update(new Models.Setting
            {
                Key = updateSettingParameter.Key,
                Value = updateSettingParameter.Value
            });
        }

        #endregion
    }
}
