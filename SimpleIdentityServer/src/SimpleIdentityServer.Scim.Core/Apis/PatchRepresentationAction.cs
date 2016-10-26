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
        private readonly IRepresentationRequestParser _representationRequestParser;

        public PatchRepresentationAction(
            IPatchRequestParser patchRequestParser,
            IRepresentationStore representationStore,
            IApiResponseFactory apiResponseFactory,
            IFilterParser filterParser,
            IJsonParser jsonParser,
            IErrorResponseFactory errorResponseFactory,
            IRepresentationResponseParser responseParser,
            IParametersValidator parametersValidator,
            IRepresentationRequestParser representationRequestParser)
        {
            _patchRequestParser = patchRequestParser;
            _representationStore = representationStore;
            _apiResponseFactory = apiResponseFactory;
            _filterParser = filterParser;
            _jsonParser = jsonParser;
            _errorResponseFactory = errorResponseFactory;
            _responseParser = responseParser;
            _parametersValidator = parametersValidator;
            _representationRequestParser = representationRequestParser;
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

                // 4.2 Check value is filled-in.
                if ((operation.Type == PatchOperations.add || operation.Type == PatchOperations.replace) &&
                    operation.Value == null)
                {
                    return _apiResponseFactory.CreateError(
                        HttpStatusCode.BadRequest,
                         _errorResponseFactory.CreateError(ErrorMessages.TheValueNeedsToBeSpecified, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
                }

                // 4.3 Process filter & get values.
                IEnumerable<RepresentationAttribute> attrs = null;
                IEnumerable<RepresentationAttribute> filteredAttrs = null;
                if (!string.IsNullOrWhiteSpace(operation.Path))
                {
                    // 4.3.1 Process filter.
                    var filter = _filterParser.Parse(operation.Path);
                    var filtered = filter.Evaluate(representation);
                    if (filtered == null || !filtered.Any())
                    {
                        return _apiResponseFactory.CreateError(
                            HttpStatusCode.BadRequest,
                            _errorResponseFactory.CreateError(ErrorMessages.TheFilterIsNotCorrect, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidFilter)
                        );
                    }

                    // 4.3.2 Get targeted attributes.
                    var target = _filterParser.GetTarget(operation.Path);
                    var filterRepresentation = _filterParser.Parse(target);
                    attrs = filterRepresentation.Evaluate(representation);

                    if (operation.Type == PatchOperations.remove)
                    {
                        // 4.3.3 If operation = remove then values are not retrieved.
                        filteredAttrs = filtered;
                    }
                    else
                    {
                        // 4.3.4 if operation = replace or add then retrieve values.
                        var name = filtered.First().SchemaAttribute.Name;
                        var token = operation.Value.SelectToken(name);
                        if (token == null)
                        {
                            token = new JObject();
                            token[name] = operation.Value;
                        }

                        string error;
                        var value = _jsonParser.GetRepresentation(token, filtered.First().SchemaAttribute, CheckStrategies.Standard, out error);
                        if (value == null)
                        {
                            return _apiResponseFactory.CreateError(
                            HttpStatusCode.BadRequest,
                            _errorResponseFactory.CreateError(error, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
                        }
                        filteredAttrs = new[] { value };
                    }
                }

                // 4.4 If there's no filter then parse the value with the schema.
                if (filteredAttrs == null)
                {
                    if (operation.Value != null)
                    {
                        string error;
                        var repr = _representationRequestParser.Parse(operation.Value, schemaId, CheckStrategies.Standard, out error);
                        if (repr == null)
                        {
                            return _apiResponseFactory.CreateError(
                                HttpStatusCode.BadRequest,
                                _errorResponseFactory.CreateError(error, HttpStatusCode.BadRequest, Constants.ScimTypeValues.InvalidSyntax));
                        }

                        filteredAttrs = repr.Attributes;
                        attrs = representation.Attributes;
                    }
                }

                foreach (var filteredAttr in filteredAttrs)
                {
                    var attr = attrs.FirstOrDefault(a => a.SchemaAttribute.Name == filteredAttr.SchemaAttribute.Name);
                    // 4.5.1 Check mutability.
                    if (filteredAttr.SchemaAttribute.Mutability == Constants.SchemaAttributeMutability.Immutable ||
                        filteredAttr.SchemaAttribute.Mutability == Constants.SchemaAttributeMutability.ReadOnly)
                    {
                        return _apiResponseFactory.CreateError(
                            HttpStatusCode.BadRequest,
                            _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheImmutableAttributeCannotBeUpdated, filteredAttr.SchemaAttribute.Name), HttpStatusCode.BadRequest, Constants.ScimTypeValues.Mutability));
                    }                

                    // TODO : Check uniqueness.

                    switch (operation.Type)
                    {
                        // 4.5.2.1 Remove attributes.
                        case PatchOperations.remove:
                            if (attr == null)
                            {
                                return _apiResponseFactory.CreateError(
                                    HttpStatusCode.BadRequest,
                                    _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheAttributeDoesntExist, filteredAttr.SchemaAttribute.Name), HttpStatusCode.BadRequest)
                                );
                            }

                            if (filteredAttr.SchemaAttribute.MultiValued)
                            {
                                // 4.5.2.1.1 Remove attribute from array
                                if (!Remove(attr, filteredAttr))
                                {
                                    return _apiResponseFactory.CreateError(
                                        HttpStatusCode.BadRequest,
                                        _errorResponseFactory.CreateError(ErrorMessages.TheRepresentationCannotBeRemoved, HttpStatusCode.BadRequest)
                                    );
                                }
                            }
                            else
                            {
                                // 4.5.2.1.2 Remove attribute from complex representation.
                                if (attr.Parent != null)
                                {
                                    var complexParent = attr.Parent as ComplexRepresentationAttribute;
                                    if (complexParent == null)
                                    {
                                        continue;
                                    }

                                    complexParent.Values = complexParent.Values.Where(v => !v.Equals(attr));
                                }
                                else
                                {
                                    representation.Attributes = representation.Attributes.Where(v => !v.Equals(attr));
                                }
                            }
                            break;
                        // 4.5.2.2 Add attribute.
                        case PatchOperations.add:
                            if (string.IsNullOrWhiteSpace(operation.Path) && attr == null)
                            {
                                representation.Attributes = representation.Attributes.Concat(new[] { filteredAttr });
                                continue;
                            }

                            if (attr == null)
                            {
                                return _apiResponseFactory.CreateError(
                                    HttpStatusCode.BadRequest,
                                    _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheAttributeDoesntExist, filteredAttr.SchemaAttribute.Name), HttpStatusCode.BadRequest)
                                );
                            }

                            if (!filteredAttr.SchemaAttribute.MultiValued)
                            {
                                if (!Set(attr, filteredAttr))
                                {
                                    return _apiResponseFactory.CreateError(
                                        HttpStatusCode.BadRequest,
                                        _errorResponseFactory.CreateError(ErrorMessages.TheRepresentationCannotBeSet, HttpStatusCode.BadRequest)
                                    );
                                }
                            }
                            else
                            {
                                if (!Add(attr, filteredAttr))
                                {
                                    return _apiResponseFactory.CreateError(
                                        HttpStatusCode.BadRequest,
                                        _errorResponseFactory.CreateError(ErrorMessages.TheRepresentationCannotBeAdded, HttpStatusCode.BadRequest)
                                    );
                                }

                            }
                            break;
                        // 4.5.2.3 Replace attribute
                        case PatchOperations.replace:
                            if (attr == null)
                            {
                                return _apiResponseFactory.CreateError(
                                    HttpStatusCode.BadRequest,
                                    _errorResponseFactory.CreateError(string.Format(ErrorMessages.TheAttributeDoesntExist, filteredAttr.SchemaAttribute.Name), HttpStatusCode.BadRequest)
                                );
                            }

                            if (attr.SchemaAttribute.MultiValued)
                            {
                                if (!SetEnum(attr, filteredAttr))
                                {
                                    return _apiResponseFactory.CreateError(
                                        HttpStatusCode.BadRequest,
                                        _errorResponseFactory.CreateError(ErrorMessages.TheRepresentationCannotBeSet, HttpStatusCode.BadRequest)
                                    );
                                }
                            }
                            else
                            {
                                if (!Set(attr, filteredAttr))
                                {
                                    return _apiResponseFactory.CreateError(
                                        HttpStatusCode.BadRequest,
                                        _errorResponseFactory.CreateError(ErrorMessages.TheRepresentationCannotBeSet, HttpStatusCode.BadRequest)
                                    );
                                }
                            }
                            break;
                    }
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

        private bool RemoveEnum(RepresentationAttribute attr, RepresentationAttribute attrToBeRemoved)
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

                    cAttr.Values = cAttr.Values.Where(v => !attrsToBeRemoved.Contains(v));
                    break;
                default:
                    return false;
            }

            return true;
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
                    foreach (var attrToBeRemovedValue in cAttrToBeRemoved.Values)
                    {
                        var removed = cAttr.Values.FirstOrDefault(c => attrToBeRemovedValue.Equals(c));
                        if (removed == null)
                        {
                            continue;
                        }

                        attrsToBeRemoved.Add(removed);
                    }

                    cAttr.Values = cAttr.Values.Where(v => !attrsToBeRemoved.Contains(v));
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool Add(RepresentationAttribute attr, RepresentationAttribute attrToBeAdded)
        {
            switch (attr.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    var strAttr = attr as SingularRepresentationAttribute<IEnumerable<string>>;
                    var strAttrToBeAdded = attrToBeAdded as SingularRepresentationAttribute<IEnumerable<string>>;
                    if (strAttr == null || strAttrToBeAdded == null)
                    {
                        return false;
                    }

                    strAttr.Value = strAttr.Value.Concat(strAttrToBeAdded.Value);
                    break;
                case Constants.SchemaAttributeTypes.Boolean:
                    var bAttr = attr as SingularRepresentationAttribute<IEnumerable<bool>>;
                    var bAttrToBeAdded = attrToBeAdded as SingularRepresentationAttribute<IEnumerable<bool>>;
                    if (bAttr == null || bAttrToBeAdded == null)
                    {
                        return false;
                    }

                    bAttr.Value = bAttr.Value.Concat(bAttrToBeAdded.Value);
                    break;
                case Constants.SchemaAttributeTypes.DateTime:
                    var dAttr = attr as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                    var dAttrToBeAdded = attrToBeAdded as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                    if (dAttr == null || dAttrToBeAdded == null)
                    {
                        return false;
                    }

                    dAttr.Value = dAttr.Value.Concat(dAttrToBeAdded.Value);
                    break;
                case Constants.SchemaAttributeTypes.Complex:
                    var cAttr = attr as ComplexRepresentationAttribute;
                    var cAttrToBeAdded = attrToBeAdded as ComplexRepresentationAttribute;
                    if (cAttr == null || cAttrToBeAdded == null)
                    {
                        return false;
                    }
                    
                    cAttr.Values = cAttr.Values.Concat(cAttrToBeAdded.Values);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool Set(RepresentationAttribute attr, RepresentationAttribute attrToBeSet)
        {
            switch (attr.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    var strAttr = attr as SingularRepresentationAttribute<string>;
                    var strAttrToBeSet = attrToBeSet as SingularRepresentationAttribute<string>;
                    if (strAttr == null || strAttrToBeSet == null)
                    {
                        return false;
                    }

                    strAttr.Value = strAttrToBeSet.Value;
                    break;
                case Constants.SchemaAttributeTypes.Boolean:
                    var bAttr = attr as SingularRepresentationAttribute<bool>;
                    var bAttrToBeSet = attrToBeSet as SingularRepresentationAttribute<bool>;
                    if (bAttr == null || bAttrToBeSet == null)
                    {
                        return false;
                    }

                    bAttr.Value = bAttrToBeSet.Value;
                    break;
                case Constants.SchemaAttributeTypes.DateTime:
                    var dAttr = attr as SingularRepresentationAttribute<DateTime>;
                    var dAttrToBeSet = attrToBeSet as SingularRepresentationAttribute<DateTime>;
                    if (dAttr == null || dAttrToBeSet == null)
                    {
                        return false;
                    }

                    dAttr.Value = dAttrToBeSet.Value;
                    break;
                case Constants.SchemaAttributeTypes.Complex:
                    var cAttr = attr as ComplexRepresentationAttribute;
                    var cAttrToBSet = attrToBeSet as ComplexRepresentationAttribute;
                    if (cAttr == null || cAttrToBSet == null)
                    {
                        return false;
                    }

                    cAttr.Values = cAttrToBSet.Values;
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool SetEnum(RepresentationAttribute attr, RepresentationAttribute attrToBeSet)
        {
            switch (attr.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    var strAttr = attr as SingularRepresentationAttribute<IEnumerable<string>>;
                    var strAttrToBeSet = attrToBeSet as SingularRepresentationAttribute<IEnumerable<string>>;
                    if (strAttr == null || strAttrToBeSet == null)
                    {
                        return false;
                    }

                    strAttr.Value = strAttrToBeSet.Value;
                    break;
                case Constants.SchemaAttributeTypes.Boolean:
                    var bAttr = attr as SingularRepresentationAttribute<IEnumerable<bool>>;
                    var bAttrToBeSet = attrToBeSet as SingularRepresentationAttribute<IEnumerable<bool>>;
                    if (bAttr == null || bAttrToBeSet == null)
                    {
                        return false;
                    }

                    bAttr.Value = bAttrToBeSet.Value;
                    break;
                case Constants.SchemaAttributeTypes.DateTime:
                    var dAttr = attr as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                    var dAttrToBeSet = attrToBeSet as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                    if (dAttr == null || dAttrToBeSet == null)
                    {
                        return false;
                    }

                    dAttr.Value = dAttrToBeSet.Value;
                    break;
                case Constants.SchemaAttributeTypes.Complex:
                    var cAttr = attr as ComplexRepresentationAttribute;
                    var cAttrToBSet = attrToBeSet as ComplexRepresentationAttribute;
                    if (cAttr == null || cAttrToBSet == null)
                    {
                        return false;
                    }

                    cAttr.Values = cAttrToBSet.Values;
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
