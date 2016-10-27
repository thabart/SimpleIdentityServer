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
    public interface IRepresentationResponseParser
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
        /// <param name="operationType">Type of operation.</param>
        /// <returns>JSON representation</returns>
        Response Parse(
            Representation representation, 
            string locationPattern, 
            string schemaId, 
            string resourceType,
            OperationTypes operationType);

        /// <summary>
        /// Filter the representations and return the result.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when representations are null.</exception>
        /// <param name="representationAttributes">Representations to filter.</param>
        /// <param name="searchParameter">Search parameters.</param>
        /// <returns>Filtered response</returns>
        IEnumerable<object> Filter(
            IEnumerable<Representation> representations,
            SearchParameter searchParameter);
    }

    public class Response
    {
        public JObject Object { get; set; }
        public string Location { get; set; }
    }

    public enum OperationTypes
    {
        Query,
        Modification
    }

    internal class RepresentationResponseParser : IRepresentationResponseParser
    {
        private readonly ISchemaStore _schemasStore;
        private readonly IParametersValidator _parametersValidator;

        public RepresentationResponseParser(
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
        /// <param name="operationType">Type of operation.</param>
        /// <returns>JSON representation</returns>
        public Response Parse(
            Representation representation, 
            string locationPattern, 
            string schemaId, 
            string resourceType,
            OperationTypes operationType)
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

            var schema = _schemasStore.GetSchema(schemaId);
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
                    // Ignore the attributes.
                    if ((attribute.Returned ==  Constants.SchemaAttributeReturned.Never) ||
                        (operationType == OperationTypes.Query && attribute.Returned == Constants.SchemaAttributeReturned.Request))
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

        /// <summary>
        /// Filter the representations and return the result.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when representations are null.</exception>
        /// <param name="representationAttributes">Representations to filter.</param>
        /// <param name="searchParameter">Search parameters.</param>
        /// <returns>Filtered response</returns>
        public IEnumerable<object> Filter(
            IEnumerable<Representation> representations,
            SearchParameter searchParameter)
        {
            if (representations == null)
            {
                throw new ArgumentNullException(nameof(representations));
            }

            IEnumerable<string> commonAttrs = new[]
            {
                Constants.MetaResponseNames.ResourceType,
                Constants.MetaResponseNames.Created,
                Constants.MetaResponseNames.LastModified,
                Constants.MetaResponseNames.Version,
                Constants.MetaResponseNames.Location
            };

            var result = new List<IEnumerable<object>>();
            foreach(var representation in representations)
            {
                var attributes = representation.Attributes;
                // 1. Apply filter on the values.
                if (searchParameter.Filter != null)
                {
                    attributes = searchParameter.Filter.Evaluate(representation);
                }

                if (!attributes.Any())
                {
                    continue;
                }

                // 2. Include & exclude the attributes.
                attributes = attributes.Where(a =>
                    (searchParameter.Attributes == null || searchParameter.Attributes.Contains(a.SchemaAttribute.Name))
                    && (searchParameter.ExcludedAttributes == null || !searchParameter.ExcludedAttributes.Contains(a.SchemaAttribute.Name)));

                // 3. Add all attributes
                var obj = new JObject();
                foreach (var token in attributes.Select(a => GetToken(a, a.SchemaAttribute)))
                {
                    obj.Add(token);
                }

                // 4. Include & exclude common attributes.
                var filteredCommonAttrs = commonAttrs.Where(a =>
                    (searchParameter.Attributes == null || searchParameter.Attributes.Contains(a))
                    && (searchParameter.ExcludedAttributes == null || !searchParameter.ExcludedAttributes.Contains(a)));
                var properties = new List<JProperty>();
                foreach(var filteredCommonAttr in filteredCommonAttrs)
                {
                    if (filteredCommonAttr == Constants.MetaResponseNames.ResourceType)
                    {
                        properties.Add(new JProperty(filteredCommonAttr, representation.ResourceType));
                    }
                    if (filteredCommonAttr == Constants.MetaResponseNames.Created)
                    {
                        properties.Add(new JProperty(filteredCommonAttr, representation.Created));
                    }
                    if (filteredCommonAttr == Constants.MetaResponseNames.LastModified)
                    {
                        properties.Add(new JProperty(filteredCommonAttr, representation.LastModified));
                    }
                    if (filteredCommonAttr == Constants.MetaResponseNames.Version)
                    {
                        properties.Add(new JProperty(filteredCommonAttr, representation.Version));
                    }
                    if (filteredCommonAttr == Constants.MetaResponseNames.Location)
                    {
                        properties.Add(new JProperty(filteredCommonAttr, "location"));
                    }
                }
                
                if (properties.Any())
                {
                    obj[Constants.ScimResourceNames.Meta] = new JObject(properties);
                }

                result.Add(obj);
            }

            return result;
        }

        private static void SetCommonAttributes(JObject jObj, string location, Representation representation, string resourceType)
        {
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
                    var array = new JArray();
                    if (complexRepresentation.Values != null)
                    {
                        foreach (var subRepresentation in complexRepresentation.Values)
                        {
                            var subComplex = subRepresentation as ComplexRepresentationAttribute;
                            if (subComplex == null)
                            {
                                throw new InvalidOperationException(ErrorMessages.TheComplexAttributeArrayShouldContainsOnlyComplexAttribute);
                            }

                            var obj = new JObject();
                            foreach (var subAttr in subComplex.Values)
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
