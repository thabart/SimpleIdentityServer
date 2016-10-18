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
    public interface IUpdateRepresentationAction
    {
        ApiActionResult Execute(string id, JObject jObj, string schemaId);
    }

    internal class UpdateRepresentationAction : IUpdateRepresentationAction
    {
        private readonly IRequestParser _requestParser;
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;

        public UpdateRepresentationAction(
            IRequestParser requestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory)
        {
            _requestParser = requestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
        }

        public ApiActionResult Execute(string id, JObject jObj, string schemaId)
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

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            // 2. Parse the request.
            var representation = _requestParser.Parse(jObj, schemaId);
            var record = _representationStore.GetRepresentation(id);

            // 3. If the representation doesn't exist then 404 is returned
            if (record == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, representation.Id));
            }

            // 4. Update attributes.
            UpdateRepresentation(record, representation);

            return null;
        }

        private ApiActionResult UpdateRepresentation(Representation source, Representation target)
        {
            foreach (var attribute in source.Attributes)
            {
                var schemaAttribute = attribute.SchemaAttribute;
                var attr = target.Attributes.FirstOrDefault(r => r.SchemaAttribute.Name == schemaAttribute.Name);
                // 4.1. If the value doesn't exist then values are applied.
                if (attr == null)
                {
                    target.Attributes = target.Attributes.Concat(new[] { attribute });
                    continue;
                }

                UpdateAttribute(attribute, attr);
            }

            return null;
        }

        private bool UpdateAttribute(RepresentationAttribute source, RepresentationAttribute target)
        {
            var complexSource = source as ComplexRepresentationAttribute;
            var complexTarget = target as ComplexRepresentationAttribute;
            if (complexTarget != null)
            {
                var schemaAttribute = complexTarget.SchemaAttribute;
                if (schemaAttribute.MultiValued)
                {
                    complexTarget = (complexTarget.Values.First() as ComplexRepresentationAttribute);
                    complexSource = (complexSource.Values.First() as ComplexRepresentationAttribute);
                    if (schemaAttribute.Mutability == Constants.SchemaAttributeMutability.Immutable)
                    {
                        if (complexTarget.Values.Count() != complexTarget.Values.Count())
                        {
                            // TODO : returns error 400 && scimetype.
                            return false;
                        }
                    }

                    complexSource.Values = complexTarget.Values;
                    return true;
                }

                foreach (var complexTargetAttr in complexTarget.Values)
                {
                    var complexSourceAttr = complexSource.Values.FirstOrDefault(v => v.SchemaAttribute.Name == complexTargetAttr.SchemaAttribute.Name);
                    // If doesn't exist then value is assigned
                    if (complexSourceAttr != null)
                    {
                        complexSource.Values = complexSource.Values.Concat(new[] { complexTargetAttr });
                        continue;
                    }

                   if (!UpdateAttribute(complexSourceAttr, complexTargetAttr))
                   {
                       return false;
                   }
                }

                return true;
            }
            
            // Returns HTTPS STATUS CODE 400 if immutable attributes are not the same.
            if (target.SchemaAttribute.Mutability == Constants.SchemaAttributeMutability.Immutable)
            {
                if (!Equals(source, target))
                {
                    // TODO : returns error 400 && scimetype.
                    return false;
                }
            }

            // Assign the values
            return AssignValues(source, target);
        }

        private static bool AssignValues(RepresentationAttribute source, RepresentationAttribute target)
        {
            var result = false;
            switch (source.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    result = AssignValues<string>(source, target);
                    break;
                case Constants.SchemaAttributeTypes.Boolean:
                    result = AssignValues<bool>(source, target);
                    break;
                case Constants.SchemaAttributeTypes.Decimal:
                    result = AssignValues<decimal>(source, target);
                    break;
                case Constants.SchemaAttributeTypes.DateTime:
                    result = AssignValues<DateTime>(source, target);
                    break;
                case Constants.SchemaAttributeTypes.Integer:
                    result = AssignValues<int>(source, target);
                    break;
            }

            return result;
        }

        private static bool AssignValues<T>(RepresentationAttribute source, RepresentationAttribute target)
        {
            if (source.SchemaAttribute.MultiValued)
            {
                var singularEnumSource = source as SingularRepresentationAttribute<IEnumerable<T>>;
                var singularEnumTarget = target as SingularRepresentationAttribute<IEnumerable<T>>;
                if (singularEnumSource == null || singularEnumTarget == null)
                {
                    return false;
                }

                singularEnumSource.Value = singularEnumTarget.Value;
                return true;
            }

            var singularSource = source as SingularRepresentationAttribute<T>;
            var singularTarget = target as SingularRepresentationAttribute<T>;
            if (singularSource == null || singularTarget == null)
            {
                return false;
            }

            singularSource.Value = singularTarget.Value;
            return true;
        }

        private static bool Equals(RepresentationAttribute source, RepresentationAttribute target)
        {
            switch (source.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    return Equals<string>(source, target);
                case Constants.SchemaAttributeTypes.Boolean:
                    return Equals<bool>(source, target);
                case Constants.SchemaAttributeTypes.Decimal:
                    return Equals<decimal>(source, target);
                case Constants.SchemaAttributeTypes.DateTime:
                    return Equals<DateTime>(source, target);
                case Constants.SchemaAttributeTypes.Integer:
                    return Equals<int>(source, target);
                default:
                    return false;
            }
        }

        private static bool Equals<T>(RepresentationAttribute source, RepresentationAttribute target)
        {
            if (source.SchemaAttribute.MultiValued)
            {
                var singularEnumSource = source as SingularRepresentationAttribute<IEnumerable<T>>;
                var singularEnumTarget = target as SingularRepresentationAttribute<IEnumerable<T>>;
                if (singularEnumSource == null || singularEnumTarget == null)
                {
                    return false;
                }

                return singularEnumSource.Value.Equals(singularEnumTarget.Value);
            }
            
            var singularSource = source as SingularRepresentationAttribute<T>;
            var singularTarget = target as SingularRepresentationAttribute<T>;
            if (singularSource == null || singularTarget == null)
            {
                return false;
            }

            return singularSource.Value.Equals(singularTarget.Value);
        }
    }
}
