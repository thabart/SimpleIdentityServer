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
using SimpleIdentityServer.Uma.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ConfigurationController.Actions
{
    public interface IGetConfigurationAction
    {
        Task<ConfigurationResponse> Execute();
    }
    
    public class GetConfigurationAction : IGetConfigurationAction
    {
        private readonly List<string> _patsupportedProfiles = new List<string>
        {
            "bearer"
        };
        private readonly List<string> _aatsupportedProfiles = new List<string>
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
        private readonly List<string> _umaProfilesSupported = new List<string>
        {
            "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
        };        
        private const string RegisterApi = "/connect/register";
        private const string TokenApi = "/token";
        private const string AuthorizeApi = "/authorize";
        private const string ResourceSetApi = "/rs/resource_set";
        private const string PermissionApi = "/perm";
        private const string RptApi = "/rpt";
        private const string PolicyApi = "/policies";
        private const string IntrospectionApi = "/status";
        private const string ScopeApi = "/scopes";
        private readonly IHostingProvider _hostingProvider;
        private readonly IConfigurationService _configurationService;

        public GetConfigurationAction(IHostingProvider hostingProvider, IConfigurationService configurationService)
        {
            _hostingProvider = hostingProvider;
            _configurationService = configurationService;
        }
        
        public async Task<ConfigurationResponse> Execute()
        {
            var absoluteUriWithVirtualPath = _hostingProvider.GetAbsoluteUriWithVirtualPath();
            var result = new ConfigurationResponse
            {
                Version = "1.0",
                Issuer = absoluteUriWithVirtualPath,
                PatProfilesSupported = _patsupportedProfiles,
                AatProfilesSupported = _aatsupportedProfiles,
                RptProfilesSupported = _bearerRptProfiles,
                PatGrantTypesSupported = _patGrantTypesSupported,
                AatGrantTypesSupported = _aatGrantTypesSupported,
                ClaimTokenProfilesSupported = new List<string>(),
                UmaProfilesSupported = _umaProfilesSupported,
                DynamicClientEndPoint = await _configurationService.GetRegisterOperation().ConfigureAwait(false),
                TokenEndPoint = await _configurationService.GetTokenOperation().ConfigureAwait(false),
                AuthorizationEndPoint = await _configurationService.GetAuthorizationOperation().ConfigureAwait(false),
                RequestingPartyClaimsEndPoint = string.Empty,
                IntrospectionEndPoint = absoluteUriWithVirtualPath + IntrospectionApi,
                ResourceSetRegistrationEndPoint = absoluteUriWithVirtualPath + ResourceSetApi,
                PermissionRegistrationEndPoint = absoluteUriWithVirtualPath + PermissionApi,
                RptEndPoint = absoluteUriWithVirtualPath + RptApi,
                PolicyEndPoint = absoluteUriWithVirtualPath + PolicyApi,
                ScopeEndPoint = absoluteUriWithVirtualPath + ScopeApi
            };
            
            return result;
        }
    }
}