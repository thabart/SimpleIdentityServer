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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Parameters;
using SimpleIdentityServer.Configuration.DTOs.Requests;
using SimpleIdentityServer.Configuration.DTOs.Responses;
using SimpleIdentityServer.Configuration.Host.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApiContrib.Core.Storage;

namespace SimpleIdentityServer.Configuration.Extensions
{
    public static class MappingExtensions
    {
        public static UpdateSettingParameter ToParameter(this UpdateSettingRequest request)
        {
            return new UpdateSettingParameter
            {
                Key = request.Key,
                Value = request.Value
            };
        }

        public static GetBulkSettingsParameter ToParameter(this GetSettingsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new GetBulkSettingsParameter
            {
                Ids = request.Ids
            };
        }

        public static SettingResponse ToDto(this Setting setting)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            return new SettingResponse
            {
                Key = setting.Key,
                Value = setting.Value
            };
        }

        public static RepresentationResponse ToDto(this Record record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var result = new RepresentationResponse
            {
                AbsoluteExpiration = record.AbsoluteExpiration,
                SlidingExpiration = record.SlidingExpiration,
                Key = record.Key
            };

            if (record.Obj != null)
            {
                var jObj = record.Obj as JObject;
                if (jObj != null)
                {
                    result.Etag = jObj.GetValue("Etag").ToString();
                    DateTime dateTime;
                    if (DateTime.TryParse(jObj.GetValue("DateTime").ToString(), out dateTime))
                    {
                        result.DateTime = dateTime;
                    }
                }
            }

            return result;
        }
        
        public static List<SettingResponse> ToDtos(this ICollection<Setting> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return settings.Select(s => s.ToDto()).ToList();
        }

        public static IEnumerable<RepresentationResponse> ToDtos(this IEnumerable<Record> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            return records.Select(r => r.ToDto());
        }
    }
}
