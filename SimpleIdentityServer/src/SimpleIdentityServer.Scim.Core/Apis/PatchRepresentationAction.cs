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
    public interface IPatchRepresentationAction
    {
        ApiActionResult Execute(string id, JObject jObj, string schemaId, string locationPattern, string resourceType);
    }

    internal class PatchRepresentationAction : IPatchRepresentationAction
    {
        private readonly IPatchRequestParser _patchRequestParser;
        private readonly IRepresentationStore _representationStore;
        private readonly IApiResponseFactory _apiResponseFactory;
        private readonly IFilterParser _filterParser;
        private readonly IJsonParser _jsonParser;
        private readonly IErrorResponseFactory _errorResponseFactory;
        private readonly IRepresentationResponseParser _responseParser;
        private readonly IParametersValidator _parametersValidator;

        public PatchRepresentationAction(
            IPatchRequestParser patchRequestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory,
            IFilterParser filterParser,
            IJsonParser jsonParser,
            IErrorResponseFactory errorResponseFactory,
            IRepresentationResponseParser responseParser,
            IParametersValidator parametersValidator)
        {
            _patchRequestParser = patchRequestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
            _filterParser = filterParser;
            _jsonParser = jsonParser;
            _errorResponseFactory = errorResponseFactory;
            _responseParser = responseParser;
            _parametersValidator = parametersValidator;
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
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            // 2. Check representation exists
            var representation = _representationStore.GetRepresentation(id);
            if (representation == null)
            {
                return _apiResponseFactory.CreateError(
                    HttpStatusCode.NotFound,
                    string.Format(ErrorMessages.TheResourceDoesntExist, id));
            }

            // 3. Get patch operations.
            ErrorResponse errorResponse;
            var operations = _patchRequestParser.Parse(jObj, out errorResponse);
            if (operations == null)
            {
                return _apiResponseFactory.CreateError(
                    (HttpStatusCode)errorResponse.Status,
                    errorResponse);
            }

            // 4. Process operations.
            foreach (var operation in operations)
            {
                // 4.1 Check path is filled-in.
                if (operation.Type == PatchOperations.remove
                    && string.IsNullOrWhiteSpace(operation.Path))
                {
                    return _apiResponseFactory.CreateError(
                        HttpStatusCode.BadRequest,
                         _errorResponseFactory.CreateError(ErrorMessages.ThePathNeedsToBeSpecified, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
                }

                // 4.2 Process filter and execute operation.
                var attrs = representation.Attributes;
                var filteredAttrs = representation.Attributes;
                if (!string.IsNullOrWhiteSpace(operation.Path))
                {
                    var filter = _filterParser.Parse(operation.Path);
                    filteredAttrs = filter.Evaluate(representation);
                    if (filteredAttrs == null || !filteredAttrs.Any())
                    {
                        return _apiResponseFactory.CreateError(
                            HttpStatusCode.BadRequest,
                            _errorResponseFactory.CreateError(ErrorMessages.TheFilterIsNotCorrect, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidFilter)
                        );
                    }

                    var target = _filterParser.GetTarget(operation.Path);
                    var filterRepresentation = _filterParser.Parse(target);
                    attrs = filterRepresentation.Evaluate(representation);
                }

                var filteredAttr = filteredAttrs.First();
                var attr = attrs.First();

                // Check mutability.
                if (filteredAttr.SchemaAttribute.Mutability == Constants.SchemaAttributeMutability.Immutable ||
                    filteredAttr.SchemaAttribute.Mutability == Constants.SchemaAttributeMutability.ReadOnly)
                {
                    return _apiResponseFactory.CreateError(
                        HttpStatusCode.BadRequest,
                        _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, filteredAttr.SchemaAttribute.Name), HttpStatusCode.BadRequest, Constants.ScimTypeValues.Mutability));
                }

                RepresentationAttribute value;
                if (operation.Value != null)
                {
                    value = _jsonParser.GetRepresentation(operation.Value, filteredAttr.SchemaAttribute);
                }

                // Check uniqueness
                // TODO.

                switch (operation.Type)
                {
                    // 4.2 Remove attributes.
                    case PatchOperations.remove:
                        if (!attr.SchemaAttribute.MultiValued)
                        {
                            return _apiResponseFactory.CreateError(
                                HttpStatusCode.BadRequest,
                                _errorResponseFactory.CreateError(ErrorMessages.TheRepresentationCannotBeRemovedBecauseItsNotAnArray, HttpStatusCode.BadRequest)
                            );
                        }

                        if (!Remove(attr, filteredAttr))
                        {
                            return null;
                        }
                        break;
                }
            }

            // 5. Save the representation.
            _representationStore.UpdateRepresentation(representation);

            // 6. Returns the JSON representation.
            // TODO : replace locationPattern.
            var response = _responseParser.Parse(representation, locationPattern, schemaId, resourceType);
            return _apiResponseFactory.CreateResultWithContent(HttpStatusCode.OK,
                response.Object,
                response.Location);
        }

        private bool Remove(RepresentationAttribute attr, RepresentationAttribute attrToBeRemoved)
        {
            switch (attr.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    var strAttr = attr as SingularRepresentationAttribute<IEnumerable<string>>;
                    var strAttrToBeRemoved = attrToBeRemoved as SingularRepresentationAttribute<IEnumerable<string>>;
                    if (strAttr == null || strAttrToBeRemoved == null)
                    {
                        return false;
                    }

                    strAttr.Value = strAttr.Value.Except(strAttrToBeRemoved.Value);
                    break;
                case Constants.SchemaAttributeTypes.Boolean:
                    var bAttr = attr as SingularRepresentationAttribute<IEnumerable<bool>>;
                    var bAttrToBeRemoved = attrToBeRemoved as SingularRepresentationAttribute<IEnumerable<bool>>;
                    if (bAttr == null || bAttrToBeRemoved == null)
                    {
                        return false;
                    }

                    bAttr.Value = bAttr.Value.Except(bAttrToBeRemoved.Value);
                    break;
                case Constants.SchemaAttributeTypes.DateTime:
                    var dAttr = attr as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                    var dAttrToBeRemoved = attrToBeRemoved as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                    if (dAttr == null || dAttrToBeRemoved == null)
                    {
                        return false;
                    }

                    dAttr.Value = dAttr.Value.Except(dAttrToBeRemoved.Value);
                    break;
                case Constants.SchemaAttributeTypes.Complex:
                    var cAttr = attr as ComplexRepresentationAttribute;
                    var cAttrToBeRemoved = attrToBeRemoved as ComplexRepresentationAttribute;
                    if (cAttr == null || cAttrToBeRemoved == null)
                    {
                        return false;
                    }

                    var attrsToBeRemoved = new List<RepresentationAttribute>();
                    foreach(var attrToBeRemovedValue in cAttrToBeRemoved.Values)
                    {
                        var removed = cAttr.Values.FirstOrDefault(c => attrToBeRemovedValue.Equals(c));
                        if (removed == null)
                        {
                            continue;
                        }

                        attrsToBeRemoved.Add(removed);
                    }

                    cAttr.Values = cAttr.Values.Except(attrsToBeRemoved);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
