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
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IBulkRequestParser
    {
        /// <summary>
        /// Parse the JSON and return the bulk request.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a REQUIRED parameter is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the 'baseUrlPattern' parameter  is not correctly formatted.</exception>
        /// <param name="jObj">JSON that will be parsed.</param>
        /// <param name="baseUrlPattern">Base url pattern.</param>
        /// <param name="errorResponse">Error.</param>
        /// <returns>Bulk request or null.</returns>
        BulkResult Parse(JObject jObj, string baseUrlPattern, out ErrorResponse errorResponse);
    }

    internal class BulkRequestParser : IBulkRequestParser
    {
        private readonly IErrorResponseFactory _errorResponseFactory;
        private readonly ISchemaStore _schemaStore;

        public BulkRequestParser(
            IErrorResponseFactory errorResponseFactory,
            ISchemaStore schemaStore)
        {
            _errorResponseFactory = errorResponseFactory;
            _schemaStore = schemaStore;
        }

        /// <summary>
        /// Parse the JSON and return the bulk request.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a REQUIRED parameter is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the 'baseUrlPattern' parameter  is not correctly formatted.</exception>
        /// <param name="jObj">JSON that will be parsed.</param>
        /// <param name="baseUrlPattern">Base url pattern.</param>
        /// <param name="errorResponse">Error.</param>
        /// <returns>Bulk request or null.</returns>
        public BulkResult Parse(JObject jObj, string baseUrlPattern, out ErrorResponse errorResponse)
        {
            errorResponse = null;
            // 1. Check parameters.
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            if (string.IsNullOrWhiteSpace(baseUrlPattern))
            {
                throw new ArgumentNullException(nameof(baseUrlPattern));
            }

            if (!baseUrlPattern.Contains("{rootPath}"))
            {
                throw new FormatException("the baseUrlPattern is not correctly formatted");
            }

            // 2. Parse the request.
            var obj = jObj.ToObject<BulkRequest>();
            if (!obj.Schemas.Contains(Common.Constants.Messages.Bulk))
            {
                errorResponse = _errorResponseFactory.CreateError(
                    ErrorMessages.TheRequestIsNotABulkOperation,
                    HttpStatusCode.BadRequest,
                    Common.Constants.ScimTypeValues.InvalidSyntax);
                return null;
            }

            if (obj.Operations == null)
            {
                errorResponse = _errorResponseFactory.CreateError(
                    ErrorMessages.TheOperationsParameterMustBeSpecified,
                    HttpStatusCode.BadRequest,
                    Common.Constants.ScimTypeValues.InvalidSyntax);
                return null;
            }

            var response = new BulkResult
            {
                FailOnErrors = obj.FailOnErrors               
            };

            Func<string, ErrorResponse> getBulkMethodNotSupported = (method) =>
            {
                return _errorResponseFactory.CreateError(
                        string.Format(ErrorMessages.TheBulkMethodIsNotSupported, method),
                        HttpStatusCode.BadRequest,
                        Common.Constants.ScimTypeValues.InvalidSyntax);
            };
            Func<string, IList<string>> splitPath = (path) =>
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                path = path.TrimStart('/');
                var subPaths = path.Split('/');
                if (!subPaths.Any() || subPaths.Count() > 2)
                {
                    return null;
                }

                return subPaths;
            };
            Func<IList<string>, string> extractRootPath = (subPaths) =>
            {
                if (subPaths == null)
                {
                    return null;
                }

                return subPaths[0];
            };
            Func<IList<string>, string> extractId = (subPaths) =>
            {
                if (subPaths == null || subPaths.Count() < 2)
                {
                    return null;
                }

                return subPaths[1];
            };
            Func<string, string> getResourceType = (subPath) =>
            {
                if (subPath == null)
                {
                    return null;
                }

                if (!Constants.MappingRoutePathsToResourceTypes.ContainsKey(subPath))
                {
                    return null;
                }

                return Constants.MappingRoutePathsToResourceTypes[subPath];
            };

            var schemas = _schemaStore.GetSchemas();
            var resourceTypes = schemas.Select(s => s.Name);

            // 3. Check operation parameters are correct.
            var operations = new List<BulkOperationResult>();
            foreach (var operation in obj.Operations)
            {
                try
                {
                    JObject data = operation.Data as JObject;
                    // 3.1. Check data
                    if (data == null)
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            ErrorMessages.TheBulkDataParameterMustBeSpecified,
                            HttpStatusCode.BadRequest,
                            Common.Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    // 3.2. Check method
                    var httpMethod = new HttpMethod(operation.Method);
                    if (!new [] { HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete, new HttpMethod("PATCH") }.Contains(httpMethod))
                    {
                        errorResponse = getBulkMethodNotSupported(operation.Method);
                        return null;
                    }

                    // 3.3. Check path
                    if (string.IsNullOrWhiteSpace(operation.Path))
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            ErrorMessages.TheBulkOperationPathIsRequired,
                            HttpStatusCode.BadRequest,
                            Common.Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    var subPaths = splitPath(operation.Path);
                    var rootPath = extractRootPath(subPaths);
                    var resourceId = extractId(subPaths);
                    var resourceType = getResourceType(rootPath);
                    if (string.IsNullOrWhiteSpace(resourceType) ||
                        !resourceTypes.Contains(resourceType) ||
                        (httpMethod != HttpMethod.Post && string.IsNullOrWhiteSpace(resourceId)))
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            string.Format(ErrorMessages.TheBulkOperationPathIsNotSupported, operation.Path),
                            HttpStatusCode.BadRequest,
                            Common.Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    // 3.4. Check bulkId
                    if (httpMethod == HttpMethod.Post && string.IsNullOrWhiteSpace(operation.BulkId))
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            ErrorMessages.TheBulkIdParameterMustBeSpecified,
                            HttpStatusCode.BadRequest,
                            Common.Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }


                    var schema = schemas.First(s => s.Name == resourceType);
                    operations.Add(new BulkOperationResult
                    {
                        Data = data,
                        BulkId = operation.BulkId,
                        Method = httpMethod,
                        Version = operation.Version,
                        ResourceId = resourceId,
                        SchemaId = schema.Id,
                        ResourceType = resourceType,
                        LocationPattern = baseUrlPattern.Replace("{rootPath}", rootPath) + "/{id}",
                        Path = operation.Path
                    });
                }
                catch(Exception)
                {
                    errorResponse = getBulkMethodNotSupported(operation.Method);
                    return null;
                }
            }

            response.Operations = operations;
            return response;
        }
    }
}
