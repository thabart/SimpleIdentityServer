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
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IRepresentationRequestParser
    {
        /// <summary>
        /// Parse JSON and returns its representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a parameter is null or empty.</exception>
        /// <param name="jObj">JSON</param>
        /// <param name="schemaId">Schema identifier</param>
        /// <param name="checkStrategy">Strategy used to check the parameters.</param>
        /// <returns>Representation or null.</returns>
        Task<ParseRepresentationResult> Parse(JToken jObj, string schemaId, CheckStrategies checkStrategy);
    }

    public interface IJsonParser
    {
        /// <summary>
        /// Parse json and returns the representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one of the parameter is null.</exception>
        /// <param name="jObj">JSON</param>
        /// <param name="attribute">Schema attribute</param>
        /// <param name="checkStrategy">Strategy used to check the parameters.</param>
        /// <returns>Representation or null.</returns>
        ParseRepresentationAttrResult GetRepresentation(JToken jObj, SchemaAttributeResponse attribute, CheckStrategies checkStrategy);
    }

    public enum CheckStrategies
    {
        Strong,
        Standard
    }

    public class ParseRepresentationResult
    {
        public Representation Representation { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsParsed { get; set; }
    }

    public class ParseRepresentationAttrResult
    {
        public RepresentationAttribute RepresentationAttribute { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsParsed { get; set; }
    }

    internal class RepresentationRequestParser : IRepresentationRequestParser, IJsonParser
    {
        private readonly ISchemaStore _schemasStore;

        public RepresentationRequestParser(ISchemaStore schemaStore)
        {
            _schemasStore = schemaStore;
        }

        /// <summary>
        /// Parse JSON and returns its representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong while trying to parse the JSON.</exception>
        /// <param name="jObj">JSON</param>
        /// <param name="schemaId">Schema identifier</param>
        /// <param name="checkStrategy">Strategy used to check the parameters.</param>
        /// <returns>Representation or null.</returns>
        public async Task<ParseRepresentationResult> Parse(JToken jObj, string schemaId, CheckStrategies checkStrategy)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            var schema = await _schemasStore.GetSchema(schemaId).ConfigureAwait(false);
            if (schema == null)
            {
                return new ParseRepresentationResult
                {
                    ErrorMessage = string.Format(ErrorMessages.TheSchemaDoesntExist, schemaId),
                    IsParsed = false
                };
            }

            var representation = new Representation();
            var attributes = new List<RepresentationAttribute>();
            foreach (var attribute in schema.Attributes)
            {
                // 1. Ignore the attribute with readonly mutability
                if (attribute.Mutability == Common.Constants.SchemaAttributeMutability.ReadOnly)
                {
                    continue;
                }

                // 2. Add the attribute
                var repr = GetRepresentation(jObj, attribute, checkStrategy);
                if (repr.IsParsed)
                {
                    attributes.Add(repr.RepresentationAttribute);
                }
                else
                {
                    return new ParseRepresentationResult
                    {
                        IsParsed = false,
                        ErrorMessage = repr.ErrorMessage
                    };
                }
            }

            representation.Attributes = attributes;
            return new ParseRepresentationResult
            {
                IsParsed = true,
                Representation = representation
            };
        }

        /// <summary>
        /// Parse json and returns the representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one of the parameter is null.</exception>
        /// <param name="jObj">JSON</param>
        /// <param name="attribute">Schema attribute</param>
        /// <param name="checkStrategy">Strategy used to check the parameters.</param>
        /// <returns>Representation or null.</returns>
        public ParseRepresentationAttrResult GetRepresentation(JToken jObj, SchemaAttributeResponse attribute, CheckStrategies checkStrategy)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }
            
            Action<ComplexSchemaAttributeResponse, List<RepresentationAttribute>, JToken, RepresentationAttribute> setRepresentationCallback = (attr, lst, tok, reprAttr) =>
            {
                foreach (var subAttribute in attr.SubAttributes)
                {
                    var rep = GetRepresentation(tok, subAttribute, checkStrategy);
                    if (rep.IsParsed)
                    {
                        rep.RepresentationAttribute.Parent = reprAttr;
                        lst.Add(rep.RepresentationAttribute);
                    }
                }
            };
            var token = jObj.SelectToken(attribute.Name);
            // 1. Check the attribute is required
            if (token == null)
            {
                if (attribute.Required && checkStrategy == CheckStrategies.Strong)
                {
                    return new ParseRepresentationAttrResult
                    {
                        IsParsed = false,
                        ErrorMessage = string.Format(ErrorMessages.TheAttributeIsRequired, attribute.Name)
                    };
                }

                // Add empty attribute.
                return GetEmptyToken(attribute);
            }

            // 2. Check is an array
            JArray jArr = null;
            if (attribute.MultiValued)
            {
                jArr = token as JArray;
                if (jArr == null)
                {
                    return new ParseRepresentationAttrResult
                    {
                        IsParsed = false,
                        ErrorMessage = string.Format(ErrorMessages.TheAttributeIsNotAnArray, attribute.Name)
                    };
                }
            }

            // 3. Create complex attribute
            var complexAttribute = attribute as ComplexSchemaAttributeResponse;
            if (complexAttribute != null)
            {
                var representation = new ComplexRepresentationAttribute(complexAttribute);
                var values = new List<RepresentationAttribute>();
                if (complexAttribute.MultiValued)
                {
                    // 3.1 Contains an array
                    if (jArr.Count > 0)
                    {
                        foreach (var subToken in token)
                        {
                            var subRepresentation = new ComplexRepresentationAttribute(null);
                            var subValues = new List<RepresentationAttribute>();
                            setRepresentationCallback(complexAttribute, subValues, subToken, subRepresentation);
                            subRepresentation.Values = subValues;
                            values.Add(subRepresentation);
                            subRepresentation.Parent = representation;
                        }
                    }
                    else
                    {
                        values.Add(new ComplexRepresentationAttribute(null)
                        {
                            Values = new List<RepresentationAttribute>()
                        });
                    }
                }
                else
                {
                    // 3.2 Doesn't contain array
                    setRepresentationCallback(complexAttribute, values, token, representation);
                }

                representation.Values = values;
                return new ParseRepresentationAttrResult
                {
                    IsParsed = true,
                    RepresentationAttribute = representation
                };
            }

            RepresentationAttribute result = null;
            // 4. Create singular attribute.
            // Note : Don't cast to object to avoid unecessaries boxing operations ...
            switch (attribute.Type)
            {
                case Common.Constants.SchemaAttributeTypes.String:
                    result = GetSingularToken<string>(jArr, attribute, token);
                    break;
                case Common.Constants.SchemaAttributeTypes.Boolean:
                    result = GetSingularToken<bool>(jArr, attribute, token);
                    break;
                case Common.Constants.SchemaAttributeTypes.Decimal:
                    result = GetSingularToken<decimal>(jArr, attribute, token);
                    break;
                case Common.Constants.SchemaAttributeTypes.DateTime:
                    result = GetSingularToken<DateTime>(jArr, attribute, token);
                    break;
                case Common.Constants.SchemaAttributeTypes.Integer:
                    result = GetSingularToken<int>(jArr, attribute, token);
                    break;
                default:
                    return new ParseRepresentationAttrResult
                    {
                        IsParsed = false,
                        ErrorMessage = string.Format(ErrorMessages.TheAttributeTypeIsNotSupported, attribute.Type)
                    };
            }

            if (result == null)
            {
                return new ParseRepresentationAttrResult
                {
                    IsParsed = false,
                    ErrorMessage = string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, attribute.Type)
                };
            }

            return new ParseRepresentationAttrResult
            {
                RepresentationAttribute = result,
                IsParsed = true
            };
        }

        private static RepresentationAttribute GetSingularToken<T>(JArray jArr, SchemaAttributeResponse attribute, JToken token)
        {
            try
            {
                if (jArr != null)
                {
                    return new SingularRepresentationAttribute<IEnumerable<T>>(attribute, jArr.Values<T>());
                }

                return new SingularRepresentationAttribute<T>(attribute, token.Value<T>());
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static ParseRepresentationAttrResult GetEmptyToken(SchemaAttributeResponse attr)
        {
            RepresentationAttribute result = null;
            switch (attr.Type)
            {
                case Common.Constants.SchemaAttributeTypes.String:
                case Common.Constants.SchemaAttributeTypes.Reference:
                    result = GetEmptyToken(attr, string.Empty);
                    break;
                case Common.Constants.SchemaAttributeTypes.Boolean:
                    result = GetEmptyToken(attr, default(bool));
                    break;
                case Common.Constants.SchemaAttributeTypes.Decimal:
                    result = GetEmptyToken(attr, default(decimal));
                    break;
                case Common.Constants.SchemaAttributeTypes.DateTime:
                    result = GetEmptyToken(attr, default(DateTime));
                    break;
                case Common.Constants.SchemaAttributeTypes.Integer:
                    result = GetEmptyToken(attr, default(int));
                    break;
                case Common.Constants.SchemaAttributeTypes.Complex:
                    result = new ComplexRepresentationAttribute(attr);
                    break;
                default:
                    return new ParseRepresentationAttrResult
                    {
                        IsParsed = false,
                        ErrorMessage = string.Format(ErrorMessages.TheAttributeTypeIsNotSupported, attr.Type)
                    };
            }

            return new ParseRepresentationAttrResult
            {
                IsParsed = true,
                RepresentationAttribute = result
            };
        }

        private static RepresentationAttribute GetEmptyToken<T>(SchemaAttributeResponse attr, T defaultValue)
        {
            try
            {
                if (attr.MultiValued)
                {
                    return new SingularRepresentationAttribute<IEnumerable<T>>(attr, new T[0]);
                }

                return new SingularRepresentationAttribute<T>(attr, defaultValue);
            }
            catch
            {
                return null;
            }
        }
    }
}
