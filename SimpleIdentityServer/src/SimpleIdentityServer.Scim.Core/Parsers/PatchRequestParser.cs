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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IPatchRequestParser
    {
        /// <summary>
        /// Parse the object and returns the PatchOperation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the parameter is null</exception>
        /// <param name="jObj">Json that will be parsed.</param>
        /// <param name="errorResponse">Error response.</param>
        /// <returns>Patch operation or null</returns>
        IEnumerable<PatchOperation> Parse(JObject jObj, out ErrorResponse errorResponse);
    }

    internal class PatchRequestParser : IPatchRequestParser
    {
        private IErrorResponseFactory _errorResponseFactory;

        public PatchRequestParser(IErrorResponseFactory errorResponseFactory)
        {
            _errorResponseFactory = errorResponseFactory;
        }

        /// <summary>
        /// Parse the object and returns the PatchOperation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the parameter is null</exception>
        /// <param name="jObj">Json that will be parsed.</param>
        /// <param name="errorResponse">Error response.</param>
        /// <returns>Patch operation or null</returns>
        public IEnumerable<PatchOperation> Parse(JObject jObj, out ErrorResponse errorResponse)
        {
            errorResponse = null;
            // 1. Check parameter.
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            // 2. Parse the request.
            var obj = jObj.ToObject<PatchOperationsRequest>();
            if (!obj.Schemas.Contains(Constants.Messages.PatchOp))
            {
                errorResponse = _errorResponseFactory.CreateError(
                    ErrorMessages.TheRequestIsNotAPatchOperation,
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

            var result = new List<PatchOperation>();
            foreach(var operation in obj.Operations)
            {
                PatchOperations op;
                if (!Enum.TryParse(operation.Operation, out op))
                {
                    errorResponse = _errorResponseFactory.CreateError(
                        string.Format(ErrorMessages.ThePatchOperationIsNotSupported, operation.Operation),
                        HttpStatusCode.BadRequest,
                        Constants.ScimTypeValues.InvalidSyntax);
                    return null;
                }

                result.Add(new PatchOperation
                {
                    Type = op,
                    Path = operation.Path,
                    Value = operation.Value
                });
            }

            return result;
        }
    }
}
