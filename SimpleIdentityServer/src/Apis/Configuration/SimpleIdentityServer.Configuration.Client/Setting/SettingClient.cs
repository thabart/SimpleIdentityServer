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

using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SimpleIdentityServer.Configuration.Client.DTOs.Requests;

namespace SimpleIdentityServer.Configuration.Client.Setting
{
    public interface ISettingClient
    {
        Task<SettingResponse> GetSetting(string key,
            string settingUrl);

        Task<SettingResponse> GetSettingByResolving(string key,
            string configurationUrl);

        Task<List<SettingResponse>> GetSettings(string settingUrl);

        Task<List<SettingResponse>> GetSettingsByResolving(string configurationUrl);
    }

    public class SettingClient : ISettingClient
    {
        #region Fields

        private readonly IDeleteSettingOperation _deleteSettingOperation;

        private readonly IGetSettingsOperation _getSettingsOperation;

        private readonly IGetSettingOperation _getSettingOperation;

        private readonly IUpdateSettingOperation _updateSettingOperation;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #endregion

        #region Constructor

        public SettingClient(
            IDeleteSettingOperation deleteSettingOperation,
            IGetSettingsOperation getSettingsOperation,
            IGetSettingOperation getSettingOperation,
            IUpdateSettingOperation updateSettingOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _deleteSettingOperation = deleteSettingOperation;
            _getSettingOperation = getSettingOperation;
            _getSettingsOperation = getSettingsOperation;
            _updateSettingOperation = updateSettingOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<SettingResponse> GetSetting(string key, string settingUrl)
        {
            return await _getSettingOperation.ExecuteAsync(key, settingUrl);
        }

        public async Task<SettingResponse> GetSettingByResolving(string key, string configurationUrl)
        {
            var url = await GetSettingUrl(configurationUrl);
            return await GetSetting(key, url);
        }

        public async Task<List<SettingResponse>> GetSettings(string settingUrl)
        {
            return await _getSettingsOperation.ExecuteAsync(settingUrl);
        }

        public async Task<List<SettingResponse>> GetSettingsByResolving(string configurationUrl)
        {
            var url = await GetSettingUrl(configurationUrl);
            return await GetSettings(url);
        }

        public async Task<bool> DeleteSetting(string key, string settingUrl, string accessToken)
        {
            return await _deleteSettingOperation.ExecuteAsync(key, settingUrl, accessToken);
        }

        public async Task<bool> DeleteSettingByResolving(string key, string configurationUrl, string accessToken)
        {
            var url = await GetSettingUrl(configurationUrl);
            return await DeleteSetting(key, url, accessToken);
        }

        public async Task<bool> UpdateSetting(UpdateSettingParameter parameter, string settingUrl, string accessToken)
        {
            return await _updateSettingOperation.ExecuteAsync(parameter, settingUrl, accessToken);
        }

        public async Task<bool> UpdateSettingByResolving(UpdateSettingParameter parameter, string configurationUrl, string accessToken)
        {
            var url = await GetSettingUrl(configurationUrl);
            return await UpdateSetting(parameter, url, accessToken);
        }

        #endregion

        #region Private methods

        private async Task<string> GetSettingUrl(string configurationUrl)
        {
            var configurationUri = GetUri(configurationUrl);
            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return configuration.SettingEndPoint;
        }

        #endregion

        #region Private static methods

        private static Uri GetUri(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("the url is not well formed");
            }

            return uri;
        }

        #endregion
    }
}
