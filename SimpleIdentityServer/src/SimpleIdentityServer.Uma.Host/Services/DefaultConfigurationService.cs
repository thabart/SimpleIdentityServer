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
using SimpleIdentityServer.Uma.Host.Extensions;

namespace SimpleIdentityServer.Uma.Host.Services
{
    public class DefaultConfigurationService : IConfigurationService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public DefaultConfigurationService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Task<string> DefaultLanguageAsync()
        {
            return Task.FromResult("en");
        }

        public Task<double> GetAuthorizationCodeValidityPeriodInSecondsAsync()
        {
            double result = 3600;
            return Task.FromResult(result);
        }

        public Task<string> GetIssuerNameAsync()
        {
            var request = _contextAccessor.HttpContext.Request;
            return Task.FromResult(request.GetAbsoluteUriWithVirtualPath());
        }

        public Task<double> GetTokenValidityPeriodInSecondsAsync()
        {
            double result = 3600;
            return Task.FromResult(result);
        }
    }
}
