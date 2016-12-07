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

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Host.Extensions;

namespace SimpleIdentityServer.Host.Services
{
    public class DefaultConfigurationService : IConfigurationService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public DefaultConfigurationService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string DefaultLanguage()
        {
            return "en";
        }

        public Task<string> DefaultLanguageAsync()
        {
            throw new NotImplementedException();
        }

        public double GetAuthorizationCodeValidityPeriodInSeconds()
        {
            return 3600;
        }

        public Task<double> GetAuthorizationCodeValidityPeriodInSecondsAsync()
        {
            throw new NotImplementedException();
        }

        public string GetIssuerName()
        {
            var request = _contextAccessor.HttpContext.Request;
            return request.GetAbsoluteUriWithVirtualPath();
        }

        public Task<string> GetIssuerNameAsync()
        {
            throw new NotImplementedException();
        }

        public double GetTokenValidityPeriodInSeconds()
        {
            return 3600;
        }

        public Task<double> GetTokenValidityPeriodInSecondsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
