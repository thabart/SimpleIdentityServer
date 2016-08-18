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

using Microsoft.AspNetCore.Http;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Host.Extensions;

namespace SimpleIdentityServer.Host.Configuration
{
    public class ConcreteSimpleIdentityServerConfigurator : ISimpleIdentityServerConfigurator
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISimpleIdServerConfigurationClientFactory _simpleIdServerConfigurationClientFactory;

        private readonly ConfigurationParameters _configurationParameters;

        #endregion

        #region Constructor

        public ConcreteSimpleIdentityServerConfigurator(
            IHttpContextAccessor httpContextAccessor,
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            ConfigurationParameters configurationParameters)
        {
            _httpContextAccessor = httpContextAccessor;
            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _configurationParameters = configurationParameters;
        }

        #endregion

        #region Public methods

        public string GetIssuerName()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var result = request.GetAbsoluteUriWithVirtualPath();
            return result;
        }

        /// <summary>
        /// Returns the validity of an access token or identity token in seconds
        /// </summary>
        /// <returns>Validity of an access token or identity token in seconds</returns>
        public double GetTokenValidityPeriodInSeconds()
        {
            return GetExpirationTime("TokenExpirationTime");
        }

        /// <summary>
        /// Returns the validity period of an authorization token in seconds.
        /// </summary>
        /// <returns>Validity period is seconds</returns>
        public double GetAuthorizationCodeValidityPeriodInSeconds()
        {
            return GetExpirationTime("AuthorizationCodeExpirationTime");
        }

        public string DefaultLanguage()
        {
            return "en";
        }

        #endregion

        #region Private methods

        private double GetExpirationTime(string key)
        {
            double result = 0;
            double defaultValue = 3600;
            try
            {
                var setting = _simpleIdServerConfigurationClientFactory.GetSettingClient()
                    .GetSettingByResolving(key, _configurationParameters.ConfigurationUrl)
                    .Result;
                if (setting == null || !double.TryParse(setting.Value, out result))
                {
                    return defaultValue;
                }


                return result;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion
    }
}