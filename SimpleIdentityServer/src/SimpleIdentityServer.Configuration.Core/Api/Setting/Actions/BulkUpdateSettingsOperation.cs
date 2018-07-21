﻿#region copyright
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Api.Setting.Actions
{
    using Models;

    public interface IBulkUpdateSettingsOperation
    {
        Task<bool> Execute(IEnumerable<UpdateSettingParameter> settings);
    }

    internal class BulkUpdateSettingsOperation : IBulkUpdateSettingsOperation
    {
        private readonly ISettingRepository _settingRepository;

        public BulkUpdateSettingsOperation(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<bool> Execute(IEnumerable<UpdateSettingParameter> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return await _settingRepository.Update(settings.Select(s => new Setting
            {
                Key = s.Key,
                Value = s.Value
            })).ConfigureAwait(false);
        }
    }
}
