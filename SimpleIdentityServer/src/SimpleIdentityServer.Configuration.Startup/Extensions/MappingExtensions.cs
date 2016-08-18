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

using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Parameters;
using SimpleIdentityServer.Configuration.Startup.DTOs.Requests;
using SimpleIdentityServer.Configuration.Startup.DTOs.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Configuration.Startup.Extensions
{
    public static class MappingExtensions
    {
        #region To Parameter

        public static UpdateSettingParameter ToParameter(this UpdateSettingRequest request)
        {
            return new UpdateSettingParameter
            {
                Key = request.Key,
                Value = request.Value
            };
        }

        #endregion

        #region To DTO

        public static SettingResponse ToDto(this Setting setting)
        {
            return new SettingResponse
            {
                Key = setting.Key,
                Value = setting.Value
            };
        }

        #endregion

        #region To DTOs

        public static List<SettingResponse> ToDtos(this List<Setting> settings)
        {
            return settings.Select(s => s.ToDto()).ToList();
        }

        #endregion
    }
}
