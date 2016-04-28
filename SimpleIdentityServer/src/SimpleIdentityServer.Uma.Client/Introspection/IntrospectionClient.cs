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
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Client.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Introspection
{
    public interface IIntrospectionClient
    {
        Task<IntrospectionResponse> GetIntrospectionAsync(
            string rpt,
            string introspectionUrl);

        Task<IntrospectionResponse> GetIntrospectionAsync(
            string rpt,
            Uri introspectionUri);

        Task<IntrospectionResponse> GetIntrospectionByResolvingUrlAsync(
            string rpt,
            string configurationUrl);

        Task<IntrospectionResponse> GetIntrospectionByResolvingUrlAsync(
            string rpt,
            Uri configurationUri);
    }

    internal class IntrospectionClient : IIntrospectionClient
    {
        private readonly IGetIntrospectionAction _getIntrospectionAction;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #region Constructor

        public IntrospectionClient(
            IGetIntrospectionAction getIntrospectionAction,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _getIntrospectionAction = getIntrospectionAction;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<IntrospectionResponse> GetIntrospectionAsync(string rpt, string introspectionUrl)
        {
            return await GetIntrospectionAsync(rpt, UriHelpers.GetUri(introspectionUrl));
        }

        public async Task<IntrospectionResponse> GetIntrospectionAsync(string rpt, Uri introspectionUri)
        {
            return await _getIntrospectionAction.ExecuteAsync(rpt, introspectionUri);
        }

        public async Task<IntrospectionResponse> GetIntrospectionByResolvingUrlAsync(string rpt, string configurationUrl)
        {
            return await GetIntrospectionByResolvingUrlAsync(rpt, UriHelpers.GetUri(configurationUrl));
        }

        public async Task<IntrospectionResponse> GetIntrospectionByResolvingUrlAsync(string rpt, Uri configurationUri)
        {
            var introspectionEndPoint = await GetIntrospectionEndpoint(configurationUri);
            return await GetIntrospectionAsync(rpt, introspectionEndPoint);
        }

        #endregion

        #region private methods

        private async Task<string> GetIntrospectionEndpoint(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return configuration.IntrospectionEndPoint;
        }

        #endregion
    }
}
