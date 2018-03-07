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
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IBulkAction
    {
        Task<ApiActionResult> Execute(JObject jObj, string baseUrl);
    }

    internal class BulkAction : IBulkAction
    {
        private readonly IBulkRequestParser _bulkRequestParser;
        private readonly IApiResponseFactory _apiResponseFactory;
        private readonly IAddRepresentationAction _addRepresentationAction;
        private readonly IErrorResponseFactory _errorResponseFactory;
        private readonly IDeleteRepresentationAction _deleteRepresentationAction;
        private readonly IUpdateRepresentationAction _updateRepresentationAction;
        private readonly IPatchRepresentationAction _patchRepresentationAction;

        public BulkAction(
            IBulkRequestParser bulkRequestParser,
            IApiResponseFactory apiResponseFactory,
            IAddRepresentationAction addRepresentationAction,
            IDeleteRepresentationAction deleteRepresentationAction,
            IUpdateRepresentationAction updateRepresentationAction,
            IPatchRepresentationAction patchRepresentationAction,
            IErrorResponseFactory errorResponseFactory)
        {
            _bulkRequestParser = bulkRequestParser;
            _apiResponseFactory = apiResponseFactory;
            _addRepresentationAction = addRepresentationAction;
            _errorResponseFactory = errorResponseFactory;
            _deleteRepresentationAction = deleteRepresentationAction;
            _updateRepresentationAction = updateRepresentationAction;
            _patchRepresentationAction = patchRepresentationAction;
        }

        public async Task<ApiActionResult> Execute(JObject jObj, string baseUrl)
        {
            // 1. Check parameter.
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            // 2. Parse the request.
            var bulk = await _bulkRequestParser.Parse(jObj, baseUrl);
            if (!bulk.IsParsed)
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                    bulk.ErrorResponse);
            }


            // 3. Execute bulk operation.
            var numberOfErrors = 0;
            var operationsResult = new JArray();
            foreach(var operation in bulk.BulkResult.Operations)
            {
                ApiActionResult operationResult = null;
                if (operation.Method == HttpMethod.Post)
                {
                    operationResult = await _addRepresentationAction.Execute(operation.Data, operation.LocationPattern, operation.SchemaId, operation.ResourceType);
                }
                else if (operation.Method == HttpMethod.Put)
                {
                    operationResult = await _updateRepresentationAction.Execute(operation.ResourceId, operation.Data, operation.SchemaId, operation.LocationPattern, operation.ResourceType);
                }
                else if (operation.Method == HttpMethod.Delete)
                {
                    operationResult = await _deleteRepresentationAction.Execute(operation.ResourceId);
                }
                else if (operation.Method.Method == "PATCH")
                {
                    operationResult = await _patchRepresentationAction.Execute(operation.ResourceId, operation.Data, operation.SchemaId, operation.LocationPattern);
                }
                
                // 3.2. If maximum number of errors has been reached then return an error.
                if (!operationResult.IsSucceed())
                {
                    numberOfErrors++;
                    if (bulk.BulkResult.FailOnErrors.HasValue && numberOfErrors > bulk.BulkResult.FailOnErrors)
                    {
                        return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                            _errorResponseFactory.CreateError(
                                string.Format(ErrorMessages.TheMaximumNumberOfErrorHasBeenReached, bulk.BulkResult.FailOnErrors),
                                HttpStatusCode.InternalServerError,
                                Common.Constants.ScimTypeValues.TooMany));
                    }
                }

                operationsResult.Add(CreateOperationResponse(operationResult, operation));
            }

            var response = CreateResponse(operationsResult);
            return _apiResponseFactory.CreateResultWithContent(HttpStatusCode.OK, response);
        }

        private JObject CreateResponse(JArray operationsResult)
        {
            var result = new JObject();
            var schemas = new JArray();
            schemas.Add(Common.Constants.Messages.BulkResponse);
            result.Add(Common.Constants.ScimResourceNames.Schemas, schemas);
            result.Add(Common.Constants.PatchOperationsRequestNames.Operations, operationsResult);
            return result;
        }

        private JObject CreateOperationResponse(ApiActionResult apiActionResult, BulkOperationResult bulkOperation)
        {
            var result = new JObject();
            result[Common.Constants.BulkOperationRequestNames.Method] = bulkOperation.Method.Method;
            result[Common.Constants.BulkOperationResponseNames.Status] = apiActionResult.StatusCode;
            if (!string.IsNullOrWhiteSpace(bulkOperation.BulkId))
            {
                result[Common.Constants.BulkOperationRequestNames.BulkId] = bulkOperation.BulkId;
            }

            if (!string.IsNullOrWhiteSpace(bulkOperation.Version))
            {
                result[Common.Constants.BulkOperationRequestNames.Version] = bulkOperation.Version;
            }

            if (!string.IsNullOrWhiteSpace(bulkOperation.Path))
            {
                result[Common.Constants.BulkOperationRequestNames.Path] = bulkOperation.Path;
            }

            if (!string.IsNullOrWhiteSpace(apiActionResult.Location))
            {
                result[Common.Constants.BulkOperationResponseNames.Location] = apiActionResult.Location;
            }
            
            if (apiActionResult.Content != null)
            {
                result.Add(new JProperty(Common.Constants.BulkOperationResponseNames.Response, JObject.FromObject(apiActionResult.Content)));
            }

            return result;
        }
    }
}
