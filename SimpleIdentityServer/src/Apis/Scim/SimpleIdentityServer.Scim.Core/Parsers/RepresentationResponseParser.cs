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
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        Task<Response> Parse(
            Representation representation, 
            string location,
            string schemaId, 
            OperationTypes operationType);

        /// <summary>
        /// Filter the representations and return the result.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when representations are null.</exception>
        /// <param name="representations">Representations to filter.</param>
        /// <param name="searchParameter">Search parameters.</param>
        /// <param name="totalNumbers">Total numbers</param>
        /// <returns>Filtered response</returns>
        FilterResult Filter(IEnumerable<Representation> representations, SearchParameter searchParameter, int totalNumbers);
    }

    public class FilterResult
    {
        public int? ItemsPerPage { get; set; }
        public int? StartIndex { get; set; }
        public int TotalNumbers { get; set; }
        public JArray Values { get; set; }
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
        private readonly IEnumerable<IAttributeMapper> _attributeMappers;

        public RepresentationResponseParser(ISchemaStore schemaStore, ICommonAttributesFactory commonAttributeFactory, IEnumerable<IAttributeMapper> attributeMappers)
        {
            _schemasStore = schemaStore;
            _commonAttributesFactory = commonAttributeFactory;
            _attributeMappers = attributeMappers;
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
        public async Task<Response> Parse(Representation representation, string location, string schemaId, OperationTypes operationType)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            var schema = await _schemasStore.GetSchema(schemaId);
            if (schema == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheSchemaDoesntExist, schemaId));
            }

            if (_attributeMappers != null && _attributeMappers.Any())
            {
                await _attributeMappers.First().Map(representation, schemaId);
            }

            JObject result = new JObject();
            if (representation.Attributes != null &&
                representation.Attributes.Any())
            {
                foreach (var attribute in schema.Attributes)
                {
                    // Ignore the attributes.
                    if ((attribute.Returned == Common.Constants.SchemaAttributeReturned.Never) || (operationType == OperationTypes.Query && attribute.Returned == Common.Constants.SchemaAttributeReturned.Request))
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
            
            SetCommonAttributes(result, location, representation, schema.Id);
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
        /// <param name="representations">Representations to filter.</param>
        /// <param name="searchParameter">Search parameters.</param>
        /// <param name="totalNumbers">Total number of records</param>
        /// <returns>Filtered response</returns>
        public FilterResult Filter(IEnumerable<Representation> representations, SearchParameter searchParameter, int totalNumbers)
        {
            if (representations == null)
            {
                throw new ArgumentNullException(nameof(representations));
            }

            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            IEnumerable<string> commonAttrs = new[]
            {
                Common.Constants.MetaResponseNames.ResourceType,
                Common.Constants.MetaResponseNames.Created,
                Common.Constants.MetaResponseNames.LastModified,
                Common.Constants.MetaResponseNames.Version,
                Common.Constants.MetaResponseNames.Location
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
                // 2.1 Exclude & include certains attributes.
                IEnumerable<RepresentationAttribute> attributes = null;
                if (searchParameter.ExcludedAttributes != null && searchParameter.ExcludedAttributes.Any())
                {
                    foreach (var excludedAttrFilter in searchParameter.ExcludedAttributes)
                    {
                        var excludedAttrs = excludedAttrFilter.Evaluate(representation);
                        if (excludedAttrs == null)
                        {
                            continue;
                        }

                        foreach (var excludedAttr in excludedAttrs)
                        {
                            var excludedParent = excludedAttr.Parent as ComplexRepresentationAttribute;
                            if (excludedParent == null)
                            {
                                continue;
                            }

                            excludedParent.Values = excludedParent.Values.Where(v => !excludedAttrs.Contains(v));
                        }
                    }
                }

                if (searchParameter.Attributes != null && searchParameter.Attributes.Any())
                {
                    attributes = new List<RepresentationAttribute>();
                    foreach (var attrFilter in searchParameter.Attributes)
                    {
                        attributes = attributes.Concat(attrFilter.Evaluate(representation));
                    }
                }
                else
                {
                    attributes = representation.Attributes;
                }

                if (attributes == null || !attributes.Any())
                {
                    continue;
                }

                // 2.2 Add all attributes
                var obj = new JObject();
                foreach (JProperty token in attributes.Select(a => (JProperty)GetToken(a, a.SchemaAttribute)))
                {
                    var value = obj[token.Name];
                    if (value != null)
                    {
                        var arr = value as JArray;
                        if (arr != null)
                        {
                            arr.Add(token.Value);
                        }
                        else
                        {
                            obj[token.Name] = new JArray(value, token.Value);
                        }
                    }
                    else
                    {
                        obj.Add(token);
                    }
                }

                result.Add(obj);
            }

            var filterResult = new FilterResult();
            if (result.Count() > searchParameter.Count)
            {
                filterResult.StartIndex = searchParameter.StartIndex;
                filterResult.ItemsPerPage = searchParameter.Count;
                filterResult.Values = result;
            }
            else
            {
                filterResult.Values = result;
            }

            filterResult.TotalNumbers = totalNumbers;
            // 3. Paginate the representations.
            return filterResult;
        }

        private void SetCommonAttributes(JObject jObj, string location, Representation representation, string schema)
        {
            jObj.Add(_commonAttributesFactory.CreateIdJson(representation));
            jObj[Common.Constants.ScimResourceNames.Meta] = new JObject(_commonAttributesFactory.CreateMetaDataAttributeJson(representation, location));
            var arr = new JArray();
            arr.Add(schema);
            jObj[Common.Constants.ScimResourceNames.Schemas] = arr;
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
                if (complexRepresentation.Values != null)
                {
                    foreach (var subRepresentation in complexRepresentation.Values)
                    {
                        var subAttribute = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subRepresentation.SchemaAttribute.Name);
                        if (subAttribute == null)
                        {
                            continue;
                        }

                        properties.Add(GetToken(subRepresentation, subAttribute));
                    }
                }

                var props = new JObject(properties);
                return new JProperty(complexRepresentation.SchemaAttribute.Name, props);
            }
            
            // 3. Create singular attribute
            switch(attribute.Type)
            {
                case Common.Constants.SchemaAttributeTypes.String:
                case Common.Constants.SchemaAttributeTypes.Reference:
                    return GetSingularToken<string>(attribute, attr, attribute.MultiValued);
                case Common.Constants.SchemaAttributeTypes.Boolean:
                    return GetSingularToken<bool>(attribute, attr, attribute.MultiValued);
                case Common.Constants.SchemaAttributeTypes.Decimal:
                    return GetSingularToken<decimal>(attribute, attr, attribute.MultiValued);
                case Common.Constants.SchemaAttributeTypes.DateTime:
                    return GetSingularToken<DateTime>(attribute, attr, attribute.MultiValued);
                case Common.Constants.SchemaAttributeTypes.Integer:
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
