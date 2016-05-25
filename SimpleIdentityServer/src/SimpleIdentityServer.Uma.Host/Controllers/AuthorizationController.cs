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
using SimpleIdentityServer.Uma.Core.Api.Authorization;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Errors;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Authorization)]
    public class AuthorizationController : Controller
    {
        private readonly IAuthorizationActions _authorizationActions;

        private static Dictionary<AuthorizationPolicyResultEnum, ErrorResponse> _mappingResultWithError = new Dictionary<AuthorizationPolicyResultEnum, ErrorResponse>()
        {
            {
                AuthorizationPolicyResultEnum.NeedInfo,
                new ErrorResponse
                {
                    Error = ErrorCodes.NeedInfo,
                    ErrorDescription = ErrorDescriptions.TheAuthorizationProcessNeedsMoreInformation
                }
            },
            {
                AuthorizationPolicyResultEnum.NotAuthorized,
                new ErrorResponse
                {
                    Error = ErrorCodes.NotAuthorized,
                    ErrorDescription = ErrorDescriptions.TheClientIsNotAuthorized
                }
            },
            {
                AuthorizationPolicyResultEnum.RequestSubmitted,
                new ErrorResponse
                {
                    Error = ErrorCodes.RequestSubmitted,
                    ErrorDescription = ErrorDescriptions.TheResourceOwnerDidntGiveHisConsent
                }
            }
        };
        
        #region Constructor

        public AuthorizationController(IAuthorizationActions authorizationActions)
        {
            _authorizationActions = authorizationActions;
        }

        #endregion

        #region Public methods

        [HttpPost]
        [Authorize("Authorization")]
        public ActionResult GetAuthorization([FromBody] PostAuthorization postAuthorization)
        {
            if (postAuthorization == null)
            {
                throw new ArgumentNullException(nameof(postAuthorization));
            }

            var parameter = postAuthorization.ToParameter();
            var claims = this.GetClaims();
            var result = _authorizationActions.GetAuthorization(
                parameter,
                claims);
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

        #endregion

        #region Private static methods

        private static ActionResult GetErrorResponse(AuthorizationPolicyResultEnum authorizationPolicyResult)
        {
            var error = _mappingResultWithError[authorizationPolicyResult];
            return new ObjectResult(error)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        #endregion
    }
}
