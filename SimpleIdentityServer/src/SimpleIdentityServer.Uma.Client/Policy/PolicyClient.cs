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
using SimpleIdentityServer.Client.DTOs.Requests;
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Client.Errors;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IPolicyClient
    {
        Task<AddPolicyResponse> AddPolicyAsync(
            PostPolicy postPolicy,
            string policyUrl,
            string authorizationHeaderValue);

        Task<AddPolicyResponse> AddPolicyAsync(
            PostPolicy postPolicy,
            Uri policyUri,
            string authorizationHeaderValue);

        Task<AddResourceSetResponse> AddPolicyByResolvingUrlAsync(
            PostPolicy postPolicy,
            string configurationUrl,
            string authorizationHeaderValue);

        Task<AddResourceSetResponse> AddPolicyByResolvingUrlAsync(
            PostPolicy postPolicy,
            Uri configurationUri,
            string authorizationHeaderValue);
    }

    internal class PolicyClient : IPolicyClient
    {
        private readonly IAddPolicyOperation _addPolicyOperation;

        private readonly IGetConfigurationOperation _getConfigurationOperation;

        #region Constructor

        public PolicyClient(
            IAddPolicyOperation addPolicyOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addPolicyOperation = addPolicyOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        #endregion

        #region Public methods

        public async Task<AddPolicyResponse> AddPolicyAsync(PostPolicy postPolicy, string policyUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(policyUrl))
            {
                throw new ArgumentNullException(nameof(policyUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(policyUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUriIsNotWellFormed, policyUrl));
            }

            return await AddPolicyAsync(postPolicy, uri, authorizationHeaderValue);
        }

        public async Task<AddPolicyResponse> AddPolicyAsync(PostPolicy postPolicy, Uri policyUri, string authorizationHeaderValue)
        {
            return await _addPolicyOperation.ExecuteAsync(postPolicy,
                policyUri,
                authorizationHeaderValue);
        }

        public async Task<AddResourceSetResponse> AddPolicyByResolvingUrlAsync(PostPolicy postPolicy, string configurationUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            Uri uri = null;
            if (!Uri.TryCreate(configurationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(string.Format(ErrorDescriptions.TheUriIsNotWellFormed, configurationUrl));
            }

            return await AddPolicyByResolvingUrlAsync(postPolicy, uri, authorizationHeaderValue);
        }

        public async Task<AddResourceSetResponse> AddPolicyByResolvingUrlAsync(PostPolicy postPolicy, Uri configurationUri, string authorizationHeaderValue)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return await AddPolicyByResolvingUrlAsync(postPolicy, configuration.ResourceSetRegistrationEndPoint, authorizationHeaderValue);
        }

        #endregion
    }
}
