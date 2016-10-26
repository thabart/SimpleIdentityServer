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
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IUpdateRepresentationAction
    {
        ApiActionResult Execute(string id, JObject jObj, string schemaId, string locationPattern, string resourceType);
    }

    internal class UpdateRepresentationAction : IUpdateRepresentationAction
    {
        private readonly IRepresentationRequestParser _requestParser;
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;
        private readonly IRepresentationResponseParser _responseParser;
        private readonly IParametersValidator _parametersValidator;
        private readonly IErrorResponseFactory _errorResponseFactory;

        public UpdateRepresentationAction(
            IRepresentationRequestParser requestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory,
            IRepresentationResponseParser responseParser,
            IParametersValidator parametersValidator,
            IErrorResponseFactory errorResponseFactory)
        {
            _requestParser = requestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
            _responseParser = responseParser;
            _parametersValidator = parametersValidator;
            _errorResponseFactory = errorResponseFactory;
        }

        public ApiActionResult Execute(string id, JObject jObj, string schemaId, string locationPattern, string resourceType)
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

            _parametersValidator.ValidateLocationPattern(locationPattern);
            if(string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            // 2. Parse the request.
            string errorStr;
            var representation = _requestParser.Parse(jObj, schemaId, CheckStrategies.Strong, out errorStr);
            if (representation == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.BadRequest,
                    errorStr);
            }

            var record = _representationStore.GetRepresentation(id);

            // 3. If the representation doesn't exist then 404 is returned.
            if (record == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, id));
            }

            // 4. Update attributes.
            ErrorResponse error;
            if (!UpdateRepresentation(record, representation, out error))
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.BadRequest, error);
            }

            // 5. Store the new representation.
            record.LastModified = DateTime.UtcNow;
            if (!_representationStore.UpdateRepresentation(record))
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                   ErrorMessages.TheRepresentationCannotBeUpdated);
            }

            // 6. Parse the new representation.
            var response = _responseParser.Parse(record, locationPattern, schemaId, resourceType);
            return _apiResponseFactory.CreateResultWithContent(HttpStatusCode.OK,
                response.Object,
                response.Location);
        }

        private bool UpdateRepresentation(Representation source, Representation target, out ErrorResponse error)
        {
            error = null;
            if (source.Attributes == null)
            {
                source.Attributes = new List<RepresentationAttribute>();
            }

            foreach (var targetAttribute in target.Attributes)
            {
                var sourceAttribute = source.Attributes.FirstOrDefault(r => r.SchemaAttribute.Name == targetAttribute.SchemaAttribute.Name);
                // 1. If the attributes don't exist then set them.
                if (sourceAttribute == null)
                {
                    source.Attributes = source.Attributes.Concat(new[] { targetAttribute });
                    continue;
                }

                // 2. Update the attribute
                if (!UpdateAttribute(sourceAttribute, targetAttribute, out error))
                {
                    return false;
                }
            }

            return true;
        }

        private bool UpdateAttribute(RepresentationAttribute source, RepresentationAttribute target, out ErrorResponse error)
        {
            error = null;
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
                        // TODO : check the content.
                        if (complexTarget.Values.Count() != complexTarget.Values.Count())
                        {
                            error = _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, schemaAttribute.Name),
                                HttpStatusCode.BadRequest,
                                Constants.ScimTypeValues.Mutability);
                            return false;
                        }
                    }

                    complexSource.Values = complexTarget.Values;
                    return true;
                }

                foreach (var complexTargetAttr in complexTarget.Values)
                {
                    var complexSourceAttr = complexSource.Values.FirstOrDefault(v => v.SchemaAttribute.Name == complexTargetAttr.SchemaAttribute.Name);
                    if (complexSourceAttr != null)
                    {
                        complexSource.Values = complexSource.Values.Concat(new[] { complexTargetAttr });
                        continue;
                    }

                   if (!UpdateAttribute(complexSourceAttr, complexTargetAttr, out error))
                   {
                       return false;
                   }
                }

                return true;
            }
            
            if (target.SchemaAttribute.Mutability == Constants.SchemaAttributeMutability.Immutable)
            {
                if (!Equals(source, target))
                {
                    error = _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, target.SchemaAttribute.Name),
                        HttpStatusCode.BadRequest,
                        Constants.ScimTypeValues.Mutability);
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
