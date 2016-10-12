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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            return representation;
        }

        private RepresentationAttribute GetRepresentation(JToken jObj, SchemaAttributeResponse attribute)
        {
            Action<ComplexSchemaAttributeResponse, List<RepresentationAttribute>, JToken> setRepresentationCallback = (attr, lst, tok) =>
            {
                foreach (var subAttribute in attr.SubAttributes)
                {
                    var rep = GetRepresentation(tok, subAttribute);
                    if (rep != null)
                    {
                        lst.Add(rep);
                    }
                }
            };
            var token = jObj.SelectToken(attribute.Name);
            // 1. Check the attribute is required
            if (token == null)
            {
                if (attribute.Required)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsRequired, attribute.Name));
                }

                return null;
            }

            // 2. Check is an array
            JArray jArr = null;
            if (attribute.MultiValued)
            {
                jArr = token as JArray;
                if (jArr == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsNotAnArray, attribute.Name));
                }
            }

            // 3. Create complex attribute
            var complexAttribute = attribute as ComplexSchemaAttributeResponse;
            if (complexAttribute != null)
            {
                var representation = new ComplexRepresentationAttribute(complexAttribute.Name);
                var values = new List<RepresentationAttribute>();
                if (complexAttribute.MultiValued)
                {
                    // 3.1 Contains an array
                    foreach (var subToken in token)
                    {
                        var subRepresentation = new ComplexRepresentationAttribute(string.Empty);
                        var subValues = new List<RepresentationAttribute>();
                        setRepresentationCallback(complexAttribute, subValues, subToken);
                        subRepresentation.Values = subValues;
                        values.Add(subRepresentation);
                    }
                }
                else
                {
                    // 3.2 Don't contain array
                    setRepresentationCallback(complexAttribute, values, token);
                }

                representation.Values = values;
                return representation;
            }

            // 4. Create singular attribute.
            // Note : Don't cast to object to avoid unecessaries boxing operations ...
            switch(attribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    try
                    {
                        if (jArr != null)
                        {
                            return new SingularRepresentationAttribute<IEnumerable<string>>(attribute.Name, jArr.Values<string>());
                        }
                        return new SingularRepresentationAttribute<string>(attribute.Name, token.Value<string>());
                    }
                    catch (FormatException)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, Constants.SchemaAttributeTypes.String));
                    }
                case Constants.SchemaAttributeTypes.Boolean:
                    try
                    {
                        if (jArr != null)
                        {
                            return new SingularRepresentationAttribute<IEnumerable<bool>>(attribute.Name, jArr.Values<bool>());
                        }
                        return new SingularRepresentationAttribute<bool>(attribute.Name, token.Value<bool>());
                    }
                    catch (FormatException)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, Constants.SchemaAttributeTypes.Boolean));
                    }
                case Constants.SchemaAttributeTypes.Decimal:
                    try
                    {
                        if (jArr != null)
                        {
                            return new SingularRepresentationAttribute<IEnumerable<decimal>>(attribute.Name, jArr.Values<decimal>());
                        }
                        return new SingularRepresentationAttribute<decimal>(attribute.Name, token.Value<decimal>());
                    }
                    catch (FormatException)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, Constants.SchemaAttributeTypes.Decimal));
                    }
                case Constants.SchemaAttributeTypes.DateTime:
                    try
                    {
                        if (jArr != null)
                        {
                            return new SingularRepresentationAttribute<IEnumerable<DateTime>>(attribute.Name, jArr.Values<DateTime>());
                        }

                        return new SingularRepresentationAttribute<DateTime>(attribute.Name, token.Value<DateTime>());
                    }
                    catch (FormatException)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, Constants.SchemaAttributeTypes.DateTime));
                    }
                case Constants.SchemaAttributeTypes.Integer:
                    try
                    {
                        if (jArr != null)
                        {
                            return new SingularRepresentationAttribute<IEnumerable<int>>(attribute.Name, jArr.Values<int>());
                        }

                        return new SingularRepresentationAttribute<int>(attribute.Name, token.Value<int>());
                    }
                    catch (FormatException)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, Constants.SchemaAttributeTypes.Integer));
                    }
                default:
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotSupported, attribute.Type));
            }
        }
    }
}
