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

using SimpleIdentityServer.Uma.Core.Providers;
using SimpleIdentityServer.Uma.Core.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.Api.ConfigurationController.Actions
{
    public interface IGetConfigurationAction
    {
        ConfigurationResponse Execute();
    }
    
    public class GetConfigurationAction
    {
        #region Fields

        private readonly List<string> _supportedProfiles = new List<string>
        {
            "bearer"
        };

        private readonly List<string> _bearerRptProfiles = new List<string>
        {
            "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
        };

        private readonly List<string> _patGrantTypesSupported = new List<string>
        {
            "authorization_code"
        };

        private readonly List<string> _aatGrantTypesSupported = new List<string>
        {
            "authorization_code"
        };

        private readonly IHostingProvider _hostingProvider;

        #endregion

        #region Constructor

        public GetConfigurationAction(IHostingProvider hostingProvider)
        {
            _hostingProvider = hostingProvider;
        }
        
        #endregion
        
        #region Public methods
        
        public ConfigurationResponse Execute()
        {
            var absoluteUriWithVirtualPath = _hostingProvider.GetAbsoluteUriWithVirtualPath();
            var result = new ConfigurationResponse
            {
                Version = "1.0",
                Issuer = absoluteUriWithVirtualPath,
                PatProfilesSupported = _supportedProfiles,
                AatProfilesSupported = _supportedProfiles,
                RtpProfilesSupported = _bearerRptProfiles,
                PatGrantTypesSupported = _patGrantTypesSupported,
                AatGrantTypesSupported = _aatGrantTypesSupported
            };
            
            return result;
        }
        
        #endregion
    }
}