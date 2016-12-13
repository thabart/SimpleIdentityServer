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

using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Api.Setting.Actions
{
    public interface IDeleteSettingAction
    {
        Task<bool> Execute(string key);
    }

    internal class DeleteSettingAction : IDeleteSettingAction
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IConfigurationEventSource _configurationEventSource;

        public DeleteSettingAction(
            ISettingRepository settingRepository,
            IConfigurationEventSource configurationEventSource)
        {
            _settingRepository = settingRepository;
            _configurationEventSource = configurationEventSource;
        }

        public async Task<bool> Execute(string key)
        {
            _configurationEventSource.StartToDropSetting(key);
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = await _settingRepository.Remove(key);
            if (result)
            {
                _configurationEventSource.FinishToDropSetting(key);
            }

            return result;
        }
    }
}
