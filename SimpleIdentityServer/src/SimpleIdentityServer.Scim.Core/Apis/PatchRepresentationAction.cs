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
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IPatchRepresentationAction
    {
        ApiActionResult Execute(string id, JObject jObj);
    }

    internal class PatchRepresentationAction : IPatchRepresentationAction
    {
        private readonly IPatchRequestParser _patchRequestParser;
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;
        private readonly IFilterParser _filterParser;
        private readonly IJsonParser _jsonParser;

        public PatchRepresentationAction(
            IPatchRequestParser patchRequestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory,
            IFilterParser filterParser,
            IJsonParser jsonParser)
        {
            _patchRequestParser = patchRequestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
            _filterParser = filterParser;
            _jsonParser = jsonParser;
        }

        public ApiActionResult Execute(string id, JObject jObj)
        {
            // 1. Check parameters.
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            // 2. Check representation exists
            var representation = _representationStore.GetRepresentation(id);
            if (representation == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, id));
            }

            // 3. Get patch operations.
            var operations = _patchRequestParser.Parse(jObj);

            // 4. Process operations.
            foreach(var operation in operations)
            {
                // 4.1 Check path is filled-in.
                if (operation.Type == Models.PatchOperations.remove 
                    && string.IsNullOrWhiteSpace(operation.Path))
                {
                    return _apiResponseFactory.CreateError(
                        HttpStatusCode.BadRequest,
                        ErrorMessages.ThePathNeedsToBeSpecified);
                }

                // 4.2 Process filter.
                var filter = _filterParser.Parse(operation.Path);
                var attrs = filter.Evaluate(representation);
                foreach (var attr in attrs)
                {
                    switch(operation.Type)
                    {
                        case PatchOperations.remove:
                            continue;
                    }
                    /*
                    var value = _jsonParser.GetRepresentation(operation.Value, attr.SchemaAttribute);
                    if (value == null)
                    {
                        continue;
                    }
                    */
                }
            }

            return null;
        }
    }
}
