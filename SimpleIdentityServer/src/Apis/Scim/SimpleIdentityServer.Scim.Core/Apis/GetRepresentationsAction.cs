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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGetRepresentationsAction
    {
        Task<ApiActionResult> Execute(string resourceType, SearchParameter searchParameter, string locationPattern);
    }

    internal class GetRepresentationsAction : IGetRepresentationsAction
    {
        private readonly IRepresentationStore _representationStore;
        private readonly IRepresentationResponseParser _representationResponseParser;
        private readonly ICommonAttributesFactory _commonAttributesFactory;
        private readonly IParametersValidator _parametersValidator;

        public GetRepresentationsAction(
            IRepresentationStore representationStore,
            IRepresentationResponseParser representationResponseParser,
            ICommonAttributesFactory commonAttributesFactory,
            IParametersValidator parametersValidator)
        {
            _commonAttributesFactory = commonAttributesFactory;
            _representationStore = representationStore;
            _representationResponseParser = representationResponseParser;
            _parametersValidator = parametersValidator;
        }

        public async Task<ApiActionResult> Execute(string resourceType, SearchParameter searchParameter, string locationPattern)
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

            _parametersValidator.ValidateLocationPattern(locationPattern);

            // 2. Get representations & add the common attributes.
            var representations = await _representationStore.SearchRepresentations(resourceType, searchParameter);
            foreach(var representation in representations)
            {
                var location = locationPattern.Replace("{id}", representation.Id);
                representation.Attributes = representation.Attributes.Concat(new[] 
                {
                    await _commonAttributesFactory.CreateMetaDataAttribute(representation, location),
                    await _commonAttributesFactory.CreateId(representation)
                });
            }

            // 3. Filter the representations.
            var result = _representationResponseParser.Filter(representations, searchParameter);

            // 4. Construct response.
            return new ApiActionResult
            {
                Content = CreateResponse(result),
                StatusCode = 200
            };
        }

        private JObject CreateResponse(FilterResult filter)
        {
            var result = new JObject();
            var schemas = new JArray();
            var content = new JArray();
            content.Add(filter.Values);
            schemas.Add(Common.Constants.Messages.ListResponse);
            result.Add(Common.Constants.ScimResourceNames.Schemas, schemas);
            result[Common.Constants.SearchParameterResponseNames.Resources] = content;
            if (filter.ItemsPerPage.HasValue)
            {
                result.Add(Common.Constants.SearchParameterResponseNames.ItemsPerPage, filter.ItemsPerPage);
            }

            if (filter.StartIndex.HasValue)
            {
                result.Add(Common.Constants.SearchParameterResponseNames.StartIndex, filter.StartIndex);
            }

            result.Add(Common.Constants.SearchParameterResponseNames.TotalResults, filter.TotalNumbers);
            return result;
        }
    }
}
