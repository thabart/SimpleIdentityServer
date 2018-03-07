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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Api.Setting
{
    public interface ISettingActions
    {
        Task<bool> DeleteSetting(string key);
        Task<ICollection<Models.Setting>> GetSettings();
        Task<Models.Setting> GetSetting(string key);
        Task<bool> UpdateSetting(UpdateSettingParameter updateSettingParameter);
        Task<IEnumerable<Models.Setting>> BulkGetSettings(GetBulkSettingsParameter parameter);
        Task<bool> BulkUpdateSettings(IEnumerable<UpdateSettingParameter> settings);
    }

    internal class SettingActions : ISettingActions
    {
        private readonly IDeleteSettingAction _deleteSettingAction;
        private readonly IGetAllSettingAction _getAllSettingAction;
        private readonly IGetSettingAction _getSettingAction;
        private readonly IUpdateSettingAction _updateSettingAction;
        private readonly IBulkGetSettingsOperation _bulkGetSettingsOperation;
        private readonly IBulkUpdateSettingsOperation _bulkUpdateSettingsOperation;

        public SettingActions(
            IDeleteSettingAction deleteSettingAction,
            IGetAllSettingAction getAllSettingAction,
            IGetSettingAction getSettingAction,
            IUpdateSettingAction updateSettingAction,
            IBulkGetSettingsOperation bulkGetSettingsOperation,
            IBulkUpdateSettingsOperation bulkUpdateSettingsOperation)
        {
            _deleteSettingAction = deleteSettingAction;
            _getAllSettingAction = getAllSettingAction;
            _getSettingAction = getSettingAction;
            _updateSettingAction = updateSettingAction;
            _bulkGetSettingsOperation = bulkGetSettingsOperation;
            _bulkUpdateSettingsOperation = bulkUpdateSettingsOperation;
        }

        public Task<bool> DeleteSetting(string key)
        {
            return _deleteSettingAction.Execute(key);
        }

        public Task<ICollection<Models.Setting>> GetSettings()
        {
            return _getAllSettingAction.Execute();
        }

        public Task<Models.Setting> GetSetting(string key)
        {
            return _getSettingAction.Execute(key);
        }

        public Task<bool> UpdateSetting(UpdateSettingParameter updateSettingParameter)
        {
            return _updateSettingAction.Execute(updateSettingParameter);
        }

        public Task<IEnumerable<Models.Setting>> BulkGetSettings(GetBulkSettingsParameter parameter)
        {
            return _bulkGetSettingsOperation.Execute(parameter);
        }

        public Task<bool> BulkUpdateSettings(IEnumerable<UpdateSettingParameter> settings)
        {
            return _bulkUpdateSettingsOperation.Execute(settings);
        }
    }
}
