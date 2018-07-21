﻿#region copyright
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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IUpdateRepresentationAction
    {
        Task<ApiActionResult> Execute(string id, JObject jObj, string schemaId, string locationPattern, string resourceType);
    }

    internal class UpdateRepresentationAction : IUpdateRepresentationAction
    {
        private readonly IRepresentationRequestParser _requestParser;
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;
        private readonly IRepresentationResponseParser _responseParser;
        private readonly IParametersValidator _parametersValidator;
        private readonly IErrorResponseFactory _errorResponseFactory;
        private readonly IFilterParser _filterParser;

        public UpdateRepresentationAction(
            IRepresentationRequestParser requestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory,
            IRepresentationResponseParser responseParser,
            IParametersValidator parametersValidator,
            IErrorResponseFactory errorResponseFactory,
            IFilterParser filterParser)
        {
            _requestParser = requestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
            _responseParser = responseParser;
            _parametersValidator = parametersValidator;
            _errorResponseFactory = errorResponseFactory;
            _filterParser = filterParser;
        }

        public async Task<ApiActionResult> Execute(string id, JObject jObj, string schemaId, string locationPattern, string resourceType)
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
            var representation = await _requestParser.Parse(jObj, schemaId, CheckStrategies.Strong).ConfigureAwait(false);
            if (!representation.IsParsed)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.BadRequest,
                    representation.ErrorMessage);
            }

            var record = await _representationStore.GetRepresentation(id).ConfigureAwait(false);

            // 3. If the representation doesn't exist then 404 is returned.
            if (record == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, id));
            }

            // 4. Update attributes.
            var allRepresentations = (await _representationStore.GetRepresentations(record.ResourceType).ConfigureAwait(false)).Where(r => r.Id != record.Id);
            ErrorResponse error;
            if (!UpdateRepresentation(record, representation.Representation, allRepresentations, out error))
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.BadRequest, error);
            }

            // 5. Store the new representation.
            record.LastModified = DateTime.UtcNow;
            record.Version = Guid.NewGuid().ToString();
            if (!await _representationStore.UpdateRepresentation(record).ConfigureAwait(false))
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                   ErrorMessages.TheRepresentationCannotBeUpdated);
            }

            // 6. Parse the new representation.
            var response = await _responseParser.Parse(record, locationPattern.Replace("{id}", id), schemaId, OperationTypes.Modification).ConfigureAwait(false);
            return _apiResponseFactory.CreateResultWithContent(HttpStatusCode.OK,
                response.Object,
                response.Location,
                record.Version,
                record.Id);
        }

        private bool UpdateRepresentation(Representation source, Representation target, IEnumerable<Representation> allRepresentations, out ErrorResponse error)
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
                if (!UpdateAttribute(sourceAttribute, targetAttribute, allRepresentations, out error))
                {
                    return false;
                }
            }

            return true;
        }

        private bool UpdateAttribute(RepresentationAttribute source, RepresentationAttribute target, IEnumerable<Representation> allRepresentations, out ErrorResponse error)
        {
            error = null;
            var complexSource = source as ComplexRepresentationAttribute;
            var complexTarget = target as ComplexRepresentationAttribute;
            if (complexTarget != null)
            {
                var schemaAttribute = complexTarget.SchemaAttribute;
                if (schemaAttribute.MultiValued)
                {
                    // Check mutability
                    if (schemaAttribute.Mutability == Common.Constants.SchemaAttributeMutability.Immutable)
                    {
                        if (complexTarget.CompareTo(complexSource) != 0)
                        {
                            error = _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, schemaAttribute.Name),
                                HttpStatusCode.BadRequest,
                                Common.Constants.ScimTypeValues.Mutability);
                            return false;
                        }
                    }
                    
                    // Check uniqueness
                    if (schemaAttribute.Uniqueness == Common.Constants.SchemaAttributeUniqueness.Server && allRepresentations != null && allRepresentations.Any())
                    {
                        var filter = _filterParser.Parse(complexTarget.FullPath);
                        var uniqueAttrs = new List<RepresentationAttribute>();
                        foreach (var records in allRepresentations.Select(r => filter.Evaluate(r)))
                        {
                            uniqueAttrs.AddRange(records);
                        }

                        if (uniqueAttrs.Any())
                        {
                            if (uniqueAttrs.Any(a => a.CompareTo(complexTarget) == 0))
                            {
                                error = _errorResponseFactory.CreateError(
                                    string.Format(ErrorMessages.TheAttributeMustBeUnique, complexTarget.SchemaAttribute.Name), 
                                    HttpStatusCode.BadRequest,
                                    Common.Constants.ScimTypeValues.Uniqueness);
                                return false;
                            }
                        }
                    }
                }

                complexSource.Values = complexTarget.Values;
                return true;
            }
            
            // Check mutability
            if (target.SchemaAttribute.Mutability == Common.Constants.SchemaAttributeMutability.Immutable)
            {
                if (source.CompareTo(target) != 0)
                {
                    error = _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, target.SchemaAttribute.Name),
                        HttpStatusCode.BadRequest,
                        Common.Constants.ScimTypeValues.Mutability);
                    return false;
                }
            }

            // Check uniqueness
            if (target.SchemaAttribute.Uniqueness == Common.Constants.SchemaAttributeUniqueness.Server && allRepresentations != null && allRepresentations.Any())
            {
                var filter = _filterParser.Parse(target.FullPath);
                var uniqueAttrs = new List<RepresentationAttribute>();
                foreach (var records in allRepresentations.Select(r => filter.Evaluate(r)))
                {
                    uniqueAttrs.AddRange(records);
                }

                if (uniqueAttrs.Any())
                {
                    if (uniqueAttrs.Any(a => a.CompareTo(target) == 0))
                    {
                        error = _errorResponseFactory.CreateError(
                            string.Format(ErrorMessages.TheAttributeMustBeUnique, target.SchemaAttribute.Name),
                            HttpStatusCode.BadRequest,
                            Common.Constants.ScimTypeValues.Uniqueness);
                        return false;
                    }
                }
            }

            // Assign the values
            return AssignValues(source, target);
        }

        private static bool AssignValues(RepresentationAttribute source, RepresentationAttribute target)
        {
            return source.SetValue(target);
        }

        private static bool Equals(RepresentationAttribute source, RepresentationAttribute target)
        {
            return source.CompareTo(target) == 0;
        }
    }
}
