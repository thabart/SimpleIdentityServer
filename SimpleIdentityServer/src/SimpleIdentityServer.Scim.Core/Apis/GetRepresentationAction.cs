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
// limitations under the License.C:\Projects\SimpleIdentityServer\SimpleIdentityServer\src\SimpleIdentityServer.Scim.Core\DTOs\
#endregion

using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGetRepresentationAction
    {
        /// <summary>
        /// Get the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when a parameter is null or empty</exception>
        /// <param name="identifier">Identifier of the representation.</param>
        /// <param name="locationPattern">Location pattern of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>Representation or null if it doesn't exist.</returns>
        ApiActionResult Execute(string identifier, string locationPattern, string schemaId, string resourceType);
    }

    internal class GetRepresentationAction : IGetRepresentationAction
    {
        private readonly IRepresentationStore _representationStore;
        private readonly IRepresentationResponseParser _responseParser;
        private readonly IApiResponseFactory _apiResponseFactory;
        private readonly IParametersValidator _parametersValidator;

        public GetRepresentationAction(
            IRepresentationStore representationStore,
            IRepresentationResponseParser responseParser,
            IApiResponseFactory apiResponseFactory,
            IParametersValidator parametersValidator)
        {
            _representationStore = representationStore;
            _responseParser = responseParser;
            _apiResponseFactory = apiResponseFactory;
            _parametersValidator = parametersValidator;
        }

        /// <summary>
        /// Get the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when a parameter is null or empty</exception>
        /// <param name="identifier">Identifier of the representation.</param>
        /// <param name="locationPattern">Location pattern of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>Representation or null if it doesn't exist.</returns>
        public ApiActionResult Execute(string identifier, string locationPattern, string schemaId, string resourceType)
        {
            // 1. Check parameters.
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            _parametersValidator.ValidateLocationPattern(locationPattern);
            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            // 2. Check representation exists.
            var representation = _representationStore.GetRepresentation(identifier);
            if (representation == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, identifier));
            }

            // 3. Parse the result and returns the representation.
            var result = _responseParser.Parse(representation, locationPattern, schemaId, resourceType, OperationTypes.Query);
            return _apiResponseFactory.CreateResultWithContent(HttpStatusCode.OK, result.Object, result.Location);
        }
    }
}
