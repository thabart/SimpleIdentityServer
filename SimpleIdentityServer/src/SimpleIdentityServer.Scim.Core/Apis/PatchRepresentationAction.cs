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
using System.Collections.Generic;
using System.Linq;
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

        public PatchRepresentationAction(
            IPatchRequestParser patchRequestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory)
        {
            _patchRequestParser = patchRequestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
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

                // 4.2 Get attributes
                GetAttributes(representation, operation.Path);
            }

            return null;
        }

        private static IEnumerable<RepresentationAttribute> GetAttributes(Representation representation, string path)
        {
            if (representation.Attributes == null ||
                !representation.Attributes.Any())
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return representation.Attributes;
            }

            var properties = path.Split('.');
            var property = properties.First();
            var attr = representation.Attributes.FirstOrDefault(a => a.SchemaAttribute.Name == property);
            if (properties.Count() == 1)
            {
                var startArrayIndex = property.IndexOf('[');
                var endArrayIndex = property.IndexOf(']');
                if (startArrayIndex > -1 && endArrayIndex > -1)
                {
                    var arrayContent = property.Substring(startArrayIndex, endArrayIndex);
                    string s = "";
                }

                return new[] { attr };
            }

            return GetAttributesByReflection(attr, properties, 1);
        }
        

        private static IEnumerable<RepresentationAttribute> GetAttributesByReflection(
            RepresentationAttribute attr, 
            IEnumerable<string> properties, 
            int index)
        {
            var property = properties.ElementAt(index);
            var startArrayIndex = property.IndexOf('[');
            var endArrayIndex = property.IndexOf(']');
            if (startArrayIndex > -1 && endArrayIndex > -1)
            {
                var arrayContent = property.Substring(startArrayIndex, endArrayIndex);
                string s = "";
            }

            return null;
        }
    }
}
