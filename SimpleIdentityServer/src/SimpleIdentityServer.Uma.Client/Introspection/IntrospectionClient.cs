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

using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.Extensions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Introspection
{
    public interface IIntrospectionClient
    {
        Task<IntrospectionResponse> Get(string rpt, string url);
        Task<IntrospectionResponse> GetByResolution(string rpt, string url);
    }

    internal class IntrospectionClient : IIntrospectionClient
    {
        private readonly IGetIntrospectionAction _getIntrospectionAction;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public IntrospectionClient(
            IGetIntrospectionAction getIntrospectionAction,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _getIntrospectionAction = getIntrospectionAction;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<IntrospectionResponse> Get(string rpt, string url)
        {
            return _getIntrospectionAction.ExecuteAsync(rpt, url);
        }

        public async Task<IntrospectionResponse> GetByResolution(string rpt, string url)
        {
            var introspectionEndPoint = await GetIntrospectionEndpoint(UriHelpers.GetUri(url));
            return await Get(rpt, introspectionEndPoint);
        }

        private async Task<string> GetIntrospectionEndpoint(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return configuration.IntrospectionEndPoint;
        }
    }
}
