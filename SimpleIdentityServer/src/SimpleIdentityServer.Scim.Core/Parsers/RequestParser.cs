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
using SimpleIdentityServer.Scim.Core.DTOs;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IRequestParser
    {
        Representation Parse(JToken jObj, string id);
    }

    internal class RequestParser : IRequestParser
    {
        private readonly ISchemaStore _schemasStore;

        public RequestParser(ISchemaStore schemaStore)
        {
            _schemasStore = schemaStore;
        }

        public Representation Parse(JToken jObj, string id)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var schema = _schemasStore.Get(id);
            if (schema == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheSchemaDoesntExist, id));
            }

            var representation = new Representation();
            var attributes = new List<RepresentationAttribute>();
            foreach(var attribute in schema.Attributes)
            {
                // 1. Ignore the attribute with readonly mutability
                if (attribute.Mutability == Constants.SchemaAttributeMutability.ReadOnly)
                {
                    continue;
                }

                // 2. Add the attribute
                attributes.Add(GetRepresentation(jObj, attribute));
            }

            representation.Attributes = attributes;
            return null;
        }

        private RepresentationAttribute GetRepresentation(JToken jObj, SchemaAttributeResponse attribute)
        {
            var token = jObj.SelectToken(attribute.Name);
            var json = token.ToString();
            // 1. Check the attribute is required
            if (token == null && attribute.Required)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsRequired, attribute.Name));
            }

            // 2. Complex attribute
            var complexAttribute = attribute as ComplexSchemaAttributeResponse;
            if (complexAttribute != null)
            {
                var representation = new ComplexRepresentationAttribute(complexAttribute.Name);
                var values = new List<RepresentationAttribute>();
                foreach (var subAttribute in complexAttribute.SubAttributes)
                {
                    values.Add(GetRepresentation(token, subAttribute));
                }

                return representation;
            }

            return new SingularRepresentationAttribute(attribute.Name, token.ToString());
        }
    }
}
