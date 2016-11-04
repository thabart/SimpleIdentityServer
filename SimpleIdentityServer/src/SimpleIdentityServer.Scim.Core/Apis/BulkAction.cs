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
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Results;
using System;
using System.Net;

namespace SimpleIdentityServer.Scim.Core.Apis
{
    public interface IBulkAction
    {
        ApiActionResult Execute(JObject jObj);
    }

    internal class BulkAction : IBulkAction
    {
        private readonly IBulkRequestParser _bulkRequestParser;
        private readonly IApiResponseFactory _apiResponseFactory;

        public BulkAction(
            IBulkRequestParser bulkRequestParser,
            IApiResponseFactory apiResponseFactory)
        {
            _bulkRequestParser = bulkRequestParser;
            _apiResponseFactory = apiResponseFactory;
        }

        public ApiActionResult Execute(JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            ErrorResponse error;
            var bulk = _bulkRequestParser.Parse(jObj, out error);
            if (bulk == null)
            {
                return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                    error);
            }

            return null;
        }
    }
}
