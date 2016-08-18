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

using SimpleIdentityServer.Configuration.Core.Api.Setting.Actions;
using SimpleIdentityServer.Configuration.Core.Parameters;
using System.Collections.Generic;

namespace SimpleIdentityServer.Configuration.Core.Api.Setting
{
    public interface ISettingActions
    {
        bool DeleteSetting(string key);

        List<Models.Setting> GetSettings();

        Models.Setting GetSetting(string key);

        bool UpdateSetting(UpdateSettingParameter updateSettingParameter);
    }

    internal class SettingActions : ISettingActions
    {
        #region Fields

        private readonly IDeleteSettingAction _deleteSettingAction;

        private readonly IGetAllSettingAction _getAllSettingAction;

        private readonly IGetSettingAction _getSettingAction;

        private readonly IUpdateSettingAction _updateSettingAction;

        #endregion

        #region Constructor

        public SettingActions(
            IDeleteSettingAction deleteSettingAction,
            IGetAllSettingAction getAllSettingAction,
            IGetSettingAction getSettingAction,
            IUpdateSettingAction updateSettingAction)
        {
            _deleteSettingAction = deleteSettingAction;
            _getAllSettingAction = getAllSettingAction;
            _getSettingAction = getSettingAction;
            _updateSettingAction = updateSettingAction;
        }

        #endregion

        #region Public methods

        public bool DeleteSetting(string key)
        {
            return _deleteSettingAction.Execute(key);
        }

        public List<Models.Setting> GetSettings()
        {
            return _getAllSettingAction.Execute();
        }

        public Models.Setting GetSetting(string key)
        {
            return _getSettingAction.Execute(key);
        }

        public bool UpdateSetting(UpdateSettingParameter updateSettingParameter)
        {
            return _updateSettingAction.Execute(updateSettingParameter);
        }

        #endregion
    }
}
