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

using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UmaManager.Client.Operation
{
    public interface IOperationClient
    {
        Task<SearchOperationResponse> Search(string resourceSetId, string operationUrl);

        Task<SearchOperationResponse> Search(string resourceSetId, Uri operationUri);
    }

    internal class OperationClient : IOperationClient
    {
        private readonly ISearchOperationsAction _searchOperationsAction;

        #region Constructor

        public OperationClient(ISearchOperationsAction searchOperationsAction)
        {
            _searchOperationsAction = searchOperationsAction;
        }

        #endregion

        #region Public methods

        public async Task<SearchOperationResponse> Search(string resourceSetId, string operationUrl)
        {
            if (string.IsNullOrWhiteSpace(operationUrl))
            {
                throw new ArgumentNullException(nameof(operationUrl));
            }

            Uri operationUri = null;
            if (!Uri.TryCreate(operationUrl, UriKind.Absolute, out operationUri))
            {
                throw new ArgumentException($"the url {operationUrl} is not well formed");
            }

            return await Search(resourceSetId, operationUri);
        }

        public async Task<SearchOperationResponse> Search(string resourceSetId, Uri operationUri)
        {
            if (operationUri == null)
            {
                throw new ArgumentNullException(nameof(operationUri));
            }

            return await _searchOperationsAction.ExecuteAsync(resourceSetId, operationUri);
        }

        #endregion
    }
}
