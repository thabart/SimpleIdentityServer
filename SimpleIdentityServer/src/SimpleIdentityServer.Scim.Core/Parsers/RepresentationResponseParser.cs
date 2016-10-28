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
using SimpleIdentityServer.Scim.Core.Stores;
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
        /// <param name="location">Location of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="operationType">Type of operation.</param>
        /// <returns>JSON representation</returns>
        Response Parse(
            Representation representation, 
            string location,
            string schemaId, 
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
        private readonly ICommonAttributesFactory _commonAttributesFactory;

        public RepresentationResponseParser(
            ISchemaStore schemaStore,
            ICommonAttributesFactory commonAttributeFactory)
        {
            _schemasStore = schemaStore;
            _commonAttributesFactory = commonAttributeFactory;
        }

        /// <summary>
        /// Parse the representation into JSON and returns the result.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when parameters are null empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when error occured during the parsing.</exception>
        /// <param name="representation">Representation that will be parsed.</param>
        /// <param name="location">Location of the representation</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="operationType">Type of operation.</param>
        /// <returns>JSON representation</returns>
        public Response Parse(
            Representation representation, 
            string location,
            string schemaId,
            OperationTypes operationType)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
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
            
            SetCommonAttributes(result, location, representation);
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
                                   
            var result = new JArray();
            // 1. Sort the representations.
            if (searchParameter.SortBy != null)
            {
                var comparer = new RepresentationComparer(searchParameter.SortBy);
                if (searchParameter.SortOrder == SortOrders.Ascending)
                {
                    representations = representations.OrderBy(repr => repr, comparer);
                }
                else
                {
                    representations = representations.OrderByDescending(repr => repr, comparer);
                }
            }

            // 2. Filter the representations.
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
                    (searchParameter.Attributes == null || searchParameter.Attributes.Contains(a.FullPath))
                    && (searchParameter.ExcludedAttributes == null || !searchParameter.ExcludedAttributes.Contains(a.FullPath)));

                // 3. Add all attributes
                var obj = new JObject();
                foreach (var token in attributes.Select(a => GetToken(a, a.SchemaAttribute)))
                {
                    obj.Add(token);
                }

                result.Add(obj);
            }

            // 3. Paginate the representations.
            return result.Skip(searchParameter.StartIndex - 1)
                .Take(searchParameter.Count);
        }

        private void SetCommonAttributes(JObject jObj, string location, Representation representation)
        {
            jObj.Add(_commonAttributesFactory.CreateIdJson(representation));
            jObj[Constants.ScimResourceNames.Meta] = new JObject(_commonAttributesFactory.CreateMetaDataAttributeJson(representation, location));
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

    internal class RepresentationComparer : IComparer<Representation>
    {
        private readonly Filter _filter;

        public RepresentationComparer(Filter filter)
        {
            _filter = filter;
        }

        public int Compare(Representation x, Representation y)
        {
            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var xAttrs = _filter.Evaluate(x);
            var yAttrs = _filter.Evaluate(y);
            if (xAttrs == null || !xAttrs.Any())
            {
                return -1;
            }

            if (yAttrs == null || !yAttrs.Any())
            {
                return 1;
            }

            var xAttr = xAttrs.First();
            var yAttr = yAttrs.First();
            return xAttr.CompareTo(yAttr);
        }
    }
}
