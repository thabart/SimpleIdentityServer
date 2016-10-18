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
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IResponseParser
    {
        /// <summary>
        /// Parse the representation into JSON and returns the result.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when parameters are null empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when error occured during the parsing.</exception>
        /// <param name="representation">Representation that will be parsed.</param>
        /// <param name="locationPattern">Location pattern of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>JSON representation</returns>
        Response Parse(
            Representation representation, 
            string locationPattern, 
            string schemaId, 
            string resourceType);
    }

    public class Response
    {
        public JObject Object { get; set; }
        public string Location { get; set; }
    }

    internal class ResponseParser : IResponseParser
    {
        private readonly ISchemaStore _schemasStore;
        private readonly IParametersValidator _parametersValidator;

        public ResponseParser(
            ISchemaStore schemaStore,
            IParametersValidator parametersValidator)
        {
            _schemasStore = schemaStore;
            _parametersValidator = parametersValidator;
        }

        /// <summary>
        /// Parse the representation into JSON and returns the result.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when parameters are null empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when error occured during the parsing.</exception>
        /// <param name="representation">Representation that will be parsed.</param>
        /// <param name="locationPattern">Location pattern of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>JSON representation</returns>
        public Response Parse(
            Representation representation, 
            string locationPattern, 
            string schemaId, 
            string resourceType)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            _parametersValidator.ValidateLocationPattern(locationPattern);
            if (!locationPattern.Contains("{id}"))
            {
                throw new ArgumentException(
                    string.Format(ErrorMessages.TheLocationPatternIsNotCorrect,
                    locationPattern));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            var schema = _schemasStore.Get(schemaId);
            if (schema == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheSchemaDoesntExist, schemaId));
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
                    
                    var attr = representation.Attributes.FirstOrDefault(a => a.SchemaAttribute.Name == attribute.Name);
                    var token = GetToken(attr, attribute);
                    if (token != null)
                    {
                        result.Add(token);
                    }
                }
            }

            var location = locationPattern.Replace("{id}", representation.Id);
            SetCommonAttributes(result, location, representation, resourceType);
            return new Response
            {
                Location = location,
                Object = result
            };
        }

        private static void SetCommonAttributes(JObject jObj, string location, Representation representation, string resourceType)
        {
            // TODO : set the location.
            jObj.Add(new JProperty(Constants.IdentifiedScimResourceNames.Id, representation.Id));
            var properties = new JProperty[]
            {
                new JProperty(Constants.MetaResponseNames.ResourceType, resourceType),
                new JProperty(Constants.MetaResponseNames.Created, representation.Created),
                new JProperty(Constants.MetaResponseNames.LastModified, representation.LastModified),
                new JProperty(Constants.MetaResponseNames.Version, representation.Version),
                new JProperty(Constants.MetaResponseNames.Location, location)
            };
            jObj[Constants.ScimResourceNames.Meta] = new JObject(properties);
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

                // 2.1 Complex attribute[Complex attribute]
                if (attribute.MultiValued)
                {
                    if (complexRepresentation.Values == null || !complexRepresentation.Values.Any())
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsNotAnArray, attribute.Name));
                    }

                    var array = new JArray();
                    foreach(var subRepresentation in complexRepresentation.Values)
                    {
                        var subComplex = subRepresentation as ComplexRepresentationAttribute;
                        if (subComplex == null)
                        {
                            throw new InvalidOperationException(ErrorMessages.TheComplexAttributeArrayShouldContainsOnlyComplexAttribute);
                        }

                        var obj = new JObject();
                        foreach(var subAttr in subComplex.Values)
                        {
                            var att = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subAttr.SchemaAttribute.Name);
                            if (att == null)
                            {
                                continue;
                            }

                            obj.Add(GetToken(subAttr, att));
                        }

                        array.Add(obj);
                    }

                    return new JProperty(complexRepresentation.SchemaAttribute.Name, array);
                }

                var properties = new List<JToken>();
                // 2.2 Complex attribute
                foreach(var subRepresentation in complexRepresentation.Values)
                {
                    var subAttribute = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subRepresentation.SchemaAttribute.Name);
                    if (subAttribute == null)
                    {
                        continue;
                    }

                    properties.Add(GetToken(subRepresentation, subAttribute));
                }

                var props = new JObject(properties);
                return new JProperty(complexRepresentation.SchemaAttribute.Name, props);
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

                return new JProperty(enumSingularRepresentation.SchemaAttribute.Name, enumSingularRepresentation.Value);
            }
            else
            {
                var singularRepresentation = attr as SingularRepresentationAttribute<T>;
                if (singularRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, attribute.Type));
                }

                return new JProperty(singularRepresentation.SchemaAttribute.Name, singularRepresentation.Value);
            }
        }
    }
}
