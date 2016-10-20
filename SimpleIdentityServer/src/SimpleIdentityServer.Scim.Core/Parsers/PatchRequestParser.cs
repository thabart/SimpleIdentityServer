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
using SimpleIdentityServer.Scim.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using SimpleIdentityServer.Scim.Core.Errors;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IPatchRequestParser
    {
        IEnumerable<PatchOperation> Parse(JObject jObj);
    }

    internal class PatchRequestParser : IPatchRequestParser
    {
        public IEnumerable<PatchOperation> Parse(JObject jObj)
        {
            // 1. Check parameters.
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            // 2. Parse the request.
            var obj = jObj.ToObject<PatchOperationsRequest>();
            if (!obj.Schemas.Contains(Constants.Messages.PatchOp))
            {
                throw new InvalidOperationException(ErrorMessages.TheRequestIsNotAPatchOperation);
            }

            if (obj.Operations == null)
            {
                return null;
            }

            var result = new List<PatchOperation>();
            foreach(var operation in obj.Operations)
            {
                PatchOperations op;
                if (!Enum.TryParse(operation.Operation, out op))
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.ThePatchOperationIsNotSupported,
                        operation.Operation));
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
