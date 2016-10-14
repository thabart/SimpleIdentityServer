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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using System;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IGetRepresentationAction
    {
        /// <summary>
        /// Get the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when a parameter is null or empty</exception>
        /// <param name="identifier">Identifier of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>Representation or null if it doesn't exist.</returns>
        JObject Execute(string identifier, string schemaId, string resourceType);
    }

    internal class GetRepresentationAction : IGetRepresentationAction
    {
        private readonly IRepresentationStore _representationStore;
        private readonly IResponseParser _responseParser;

        public GetRepresentationAction(
            IRepresentationStore representationStore,
            IResponseParser responseParser)
        {
            _representationStore = representationStore;
            _responseParser = responseParser;
        }

        /// <summary>
        /// Get the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when a parameter is null or empty</exception>
        /// <param name="identifier">Identifier of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>Representation or null if it doesn't exist.</returns>
        public JObject Execute(string identifier, string schemaId, string resourceType)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            var representation = _representationStore.GetRepresentation(identifier);
            if (representation == null)
            {
                return null;
            }

            return _responseParser.Parse(representation, schemaId, resourceType);
        }
    }
}
