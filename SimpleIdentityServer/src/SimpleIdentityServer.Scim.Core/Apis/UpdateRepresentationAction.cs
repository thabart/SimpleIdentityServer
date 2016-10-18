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
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IUpdateRepresentationAction
    {

    }

    internal class UpdateRepresentationAction : IUpdateRepresentationAction
    {
        private readonly IRequestParser _requestParser;
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;

        public UpdateRepresentationAction(
            IRequestParser requestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory)
        {
            _requestParser = requestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
        }

        public ApiActionResult Execute(JObject jObj, string schemaId)
        {
            // 1. Check parameters.
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            // 2. Parse the request.
            var representation = _requestParser.Parse(jObj, schemaId);
            var record = _representationStore.GetRepresentation(representation.Id);

            // 3. If the representation doesn't exist then 404 is returned
            if (record == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, representation.Id));
            }

            // 4. 

            return null;
        }
    }
}
