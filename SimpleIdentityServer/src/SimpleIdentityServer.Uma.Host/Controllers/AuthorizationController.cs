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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.Authorization;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Errors;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Authorization)]
    public class AuthorizationController : Controller
    {
        private readonly IAuthorizationActions _authorizationActions;

        private static Dictionary<AuthorizationPolicyResultEnum, Func<Core.Responses.AuthorizationResponse, ErrorResponse>> _mappingResultWithError = new Dictionary<AuthorizationPolicyResultEnum, Func<Core.Responses.AuthorizationResponse, ErrorResponse>>()
        {
            {
                AuthorizationPolicyResultEnum.NeedInfo,
                GetNeedInfo
            },
            {
                AuthorizationPolicyResultEnum.NotAuthorized,
                GetNotAuthorized
            },
            {
                AuthorizationPolicyResultEnum.RequestSubmitted,
                GetRequestSubmitted
            }
        };

        public AuthorizationController(IAuthorizationActions authorizationActions)
        {
            _authorizationActions = authorizationActions;
        }

        [HttpPost]
        [Authorize("Authorization")]
        public async Task<ActionResult> GetAuthorization([FromBody] PostAuthorization postAuthorization)
        {
            if (postAuthorization == null)
            {
                throw new ArgumentNullException(nameof(postAuthorization));
            }

            var parameter = postAuthorization.ToParameter();
            var clientId = this.GetClientId();
            var result = await _authorizationActions.GetAuthorization(parameter, clientId);
            if (result.AuthorizationPolicyResult != AuthorizationPolicyResultEnum.Authorized)
            {
                return GetErrorResponse(result.AuthorizationPolicyResult);
            }

            var content = new AuthorizationResponse
            {
                Rpt = result.Rpt
            };
            return new OkObjectResult(content);
        }

        [HttpPost("bulk")]
        [Authorize("Authorization")]
        public async Task<ActionResult> GetAuthorizations([FromBody] IEnumerable<PostAuthorization> postAuthorizations)
        {
            if (postAuthorizations == null)
            {
                throw new ArgumentNullException(nameof(postAuthorizations));
            }

            var parameters = postAuthorizations.Select(p => p.ToParameter());
            var clientId = this.GetClientId();
            var responses = await _authorizationActions.GetAuthorization(parameters, clientId);
            if (!responses.Any(r => r.AuthorizationPolicyResult == AuthorizationPolicyResultEnum.Authorized))
            {
                return new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            var content = new BulkAuthorizationResponse
            {
                Rpts = responses.Where(r => r.AuthorizationPolicyResult == AuthorizationPolicyResultEnum.Authorized)
                    .Select(r => r.Rpt)
            };
            return new OkObjectResult(content);
    }

        private static ActionResult GetErrorResponse(AuthorizationPolicyResultEnum authorizationPolicyResult)
        {
            var error = _mappingResultWithError[authorizationPolicyResult];
            return new ObjectResult(error)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        private static ErrorResponse GetNeedInfo(Core.Responses.AuthorizationResponse authorizationResponse)
        {
            return new ErrorResponse
            {
                Error = ErrorCodes.NeedInfo,
                ErrorDescription = ErrorDescriptions.TheAuthorizationProcessNeedsMoreInformation,
                ErrorDetails = authorizationResponse.ErrorDetails
            };
        }

        private static ErrorResponse GetNotAuthorized(Core.Responses.AuthorizationResponse authorizationResponse)
        {
            return new ErrorResponse
            {
                Error = ErrorCodes.NotAuthorized,
                ErrorDescription = ErrorDescriptions.TheClientIsNotAuthorized
            };
        }

        private static ErrorResponse GetRequestSubmitted(Core.Responses.AuthorizationResponse authorizationResponse)
        {
            return new ErrorResponse
            {
                Error = ErrorCodes.RequestSubmitted,
                ErrorDescription = ErrorDescriptions.TheResourceOwnerDidntGiveHisConsent
            };
        }
    }
}
