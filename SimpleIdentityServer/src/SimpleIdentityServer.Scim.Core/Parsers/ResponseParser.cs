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
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IResponseParser
    {
        JObject Parse(Representation representation, string id);
    }

    internal class ResponseParser : IResponseParser
    {
        private readonly ISchemaStore _schemasStore;

        public ResponseParser(ISchemaStore schemaStore)
        {
            _schemasStore = schemaStore;
        }

        public JObject Parse(Representation representation, string id)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
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

            JObject result = new JObject();
            if (representation.Attributes != null &&
                representation.Attributes.Any())
            {
                foreach (var attribute in schema.Attributes)
                {
                    // 1. Ignore the attribute with readonly mutability.
                    if (attribute.Mutability == Constants.SchemaAttributeMutability.ReadOnly)
                    {
                        continue;
                    }
                    
                    var attr = representation.Attributes.FirstOrDefault(a => a.Type == attribute.Name);
                    var token = GetToken(attr, attribute);
                    if (token != null)
                    {
                        result.Add(token);
                    }
                }
            }

            return result;
        }

        private static JToken GetToken(RepresentationAttribute attr, SchemaAttributeResponse attribute)
        {
            // 1. Check the attribute is required
            if (attr == null)
            {
                if (attribute.Required)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsRequired, attribute.Name));
                }

                return null;
            }

            // 2. Create complex attribute
            var complexAttribute = attribute as ComplexSchemaAttributeResponse;
            if (complexAttribute != null)
            {
                var complexRepresentation = attr as ComplexRepresentationAttribute;
                if (complexRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsNotComplex, attribute.Name));
                }

                if (attribute.MultiValued)
                {
                    ComplexRepresentationAttribute subValues;
                    if (complexRepresentation.Values == null ||
                        complexRepresentation.Values.Count() > 1 ||
                        (subValues = complexRepresentation.Values.First() as ComplexRepresentationAttribute) == null)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsNotAnArray, attribute.Name));
                    }

                    var array = new JArray();
                    foreach(var subRepresentation in subValues.Values)
                    {
                        var subAttribute = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subRepresentation.Type);
                        if (subAttribute == null)
                        {
                            continue;
                        }

                        var obj = new JObject(GetToken(subRepresentation, subAttribute));
                        array.Add(obj);
                    }

                    return new JProperty(complexRepresentation.Type, array);
                }

                var properties = new List<JToken>();
                foreach(var subRepresentation in complexRepresentation.Values)
                {
                    var subAttribute = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subRepresentation.Type);
                    if (subAttribute == null)
                    {
                        continue;
                    }

                    properties.Add(GetToken(subRepresentation, subAttribute));
                }

                var props = new JObject(properties);
                return new JProperty(complexRepresentation.Type, props);
            }
            
            // 3. Create singular attribute
            switch(attribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    return GetSingularToken<string>(attribute, attr, attribute.MultiValued);
                case Constants.SchemaAttributeTypes.Boolean:
                    return GetSingularToken<bool>(attribute, attr, attribute.MultiValued);
                case Constants.SchemaAttributeTypes.Decimal:
                    return GetSingularToken<decimal>(attribute, attr, attribute.MultiValued);
                case Constants.SchemaAttributeTypes.DateTime:
                    return GetSingularToken<DateTime>(attribute, attr, attribute.MultiValued);
                case Constants.SchemaAttributeTypes.Integer:
                    return GetSingularToken<int>(attribute, attr, attribute.MultiValued);
                default:
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotSupported, attribute.Type));
            }

        }

        private static JToken GetSingularToken<T>(SchemaAttributeResponse attribute, RepresentationAttribute attr, bool isArray)
        {
            if (isArray)
            {
                var enumSingularRepresentation = attr as SingularRepresentationAttribute<IEnumerable<T>>;
                if (enumSingularRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, attribute.Type));
                }

                return new JProperty(enumSingularRepresentation.Type, enumSingularRepresentation.Value);
            }
            else
            {
                var singularRepresentation = attr as SingularRepresentationAttribute<T>;
                if (singularRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, attribute.Type));
                }

                return new JProperty(singularRepresentation.Type, singularRepresentation.Value);
            }
        }
    }
}
