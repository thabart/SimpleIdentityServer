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

using SimpleIdentityServer.Scim.Core.DTOs;
using SimpleIdentityServer.Scim.Core.Results;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Factories
{
    public interface IApiResponseFactory
    {
        ApiActionResult CreateEmptyResult(
               HttpStatusCode status);

        ApiActionResult CreateError(
            HttpStatusCode statusCode,
            string content);
    }

    internal class ApiResponseFactory : IApiResponseFactory
    {
        public ApiActionResult CreateEmptyResult(
            HttpStatusCode status)
        {
            return new ApiActionResult
            {
                StatusCode = (int)status
            };
        }

        public ApiActionResult CreateError(
            HttpStatusCode status, 
            string detail)
        {
            return new ApiActionResult
            {
                StatusCode = (int)status,
                Content = new ErrorResponse
                {
                    Schemas = new string[]
                    {
                        Constants.SchemaUrns.Error
                    },
                    Detail = detail,
                    Status = (int)status
                }
            };
        }
    }
}
