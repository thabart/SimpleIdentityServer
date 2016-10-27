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

using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using System;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGetRepresentationsAction
    {
        ApiActionResult Execute(string resourceType, SearchParameter searchParameter);
    }

    internal class GetRepresentationsAction : IGetRepresentationsAction
    {
        private readonly IRepresentationStore _representationStore;
        private readonly IRepresentationResponseParser _representationResponseParser;

        public GetRepresentationsAction(
            IRepresentationStore representationStore,
            IRepresentationResponseParser representationResponseParser)
        {
            _representationStore = representationStore;
            _representationResponseParser = representationResponseParser;
        }

        public ApiActionResult Execute(string resourceType, SearchParameter searchParameter)
        {            
            // 1. Check parameters.
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            // 2. Get representations.
            var representations = _representationStore.GetRepresentations(resourceType);
            var result = _representationResponseParser.Filter(representations, searchParameter);
            return new ApiActionResult
            {
                Content = result,
                StatusCode = 200
            };
        }
    }
}
