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
        /// <param name="jObj">JSON that will be parsed.</param>
        /// <param name="errorResponse">Error.</param>
        /// <returns>Bulk request or null.</returns>
        BulkResult Parse(JObject jObj, out ErrorResponse errorResponse);
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
        /// <param name="jObj">JSON that will be parsed.</param>
        /// <param name="errorResponse">Error.</param>
        /// <returns>Bulk request or null.</returns>
        public BulkResult Parse(JObject jObj, out ErrorResponse errorResponse)
        {
            errorResponse = null;
            // 1. Check parameters.
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            // 2. Parse the request.
            var obj = jObj.ToObject<BulkRequest>();
            if (!obj.Schemas.Contains(Constants.Messages.Bulk))
            {
                errorResponse = _errorResponseFactory.CreateError(
                    ErrorMessages.TheRequestIsNotABulkOperation,
                    HttpStatusCode.BadRequest,
                    Constants.ScimTypeValues.InvalidSyntax);
                return null;
            }

            if (obj.Operations == null)
            {
                errorResponse = _errorResponseFactory.CreateError(
                    ErrorMessages.TheOperationsParameterMustBeSpecified,
                    HttpStatusCode.BadRequest,
                    Constants.ScimTypeValues.InvalidSyntax);
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
                        Constants.ScimTypeValues.InvalidSyntax);
            };
            Func<string, string> extractIdFromPath = (path) =>
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                var subPaths = path.Split('/');
                if (!subPaths.Any() || subPaths.Count() > 2)
                {
                    return null;
                }

                return subPaths[1];
            };
            Func<string, string> extractSchemaName = (path) =>
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                var subPaths = path.Split('/');
                if (!subPaths.Any() || subPaths.Count() > 2)
                {
                    return null;
                }

                return subPaths[0];
            };
            Func<string, string, bool> checkPath = (schemaName, path) =>
            {
                if (!path.StartsWith(schemaName))
                {
                    return false;
                }

                var id = extractIdFromPath(path);
                if (string.IsNullOrWhiteSpace(id))
                {
                    return false;
                }

                return true;
            };

            var schemas = _schemaStore.GetSchemas();
            var schemaNames = schemas.Select(s => s.Name.StartsWith("/") ? s.Name : "/"+s.Name);

            // 3. Check operation parameters are correct.
            var operations = new List<BulkOperationResult>();
            foreach (var operation in obj.Operations)
            {
                try
                {
                    // 3.1. Check data
                    if (operation.Data == null)
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            ErrorMessages.TheBulkDataParameterMustBeSpecified,
                            HttpStatusCode.BadRequest,
                            Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    // 3.2. Check method
                    var httpMethod = new HttpMethod(operation.Method);
                    if (!new [] { HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete }.Contains(httpMethod))
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
                            Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    if (!(schemaNames.Contains(operation.Path) && httpMethod == HttpMethod.Post)
                        || !(schemaNames.Any(s => checkPath(s, operation.Path) && httpMethod != HttpMethod.Post)))
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            string.Format(ErrorMessages.TheBulkOperationPathIsNotSupported, operation.Path),
                            HttpStatusCode.BadRequest,
                            Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    // 3.4. Check bulkId
                    if (httpMethod == HttpMethod.Post && string.IsNullOrWhiteSpace(operation.BulkId))
                    {
                        errorResponse = _errorResponseFactory.CreateError(
                            ErrorMessages.TheBulkIdParameterMustBeSpecified,
                            HttpStatusCode.BadRequest,
                            Constants.ScimTypeValues.InvalidSyntax);
                        return null;
                    }

                    operations.Add(new BulkOperationResult
                    {
                        Data = operation.Data,
                        BulkId = operation.BulkId,
                        Method = httpMethod,
                        Version = operation.Version,
                        ResourceId = extractIdFromPath(operation.Path),
                        SchemaName = extractSchemaName(operation.Path)
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
