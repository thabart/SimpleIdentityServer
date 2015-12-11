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
using System.Security.Claims;
using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;

using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetTokenViaImplicitWorkflowOperation
    {
        ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal principal,
            string code);
    }

    public class GetTokenViaImplicitWorkflowOperation : IGetTokenViaImplicitWorkflowOperation
    {
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;

        private IGenerateAuthorizationResponse _generateAuthorizationResponse;


        public GetTokenViaImplicitWorkflowOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _generateAuthorizationResponse = generateAuthorizationResponse;
        }

        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal principal,
            string code)
        {
            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "nonce"),
                    authorizationParameter.State);
            }

            var result = _processAuthorizationRequest.Process(
                authorizationParameter,
                principal as ClaimsPrincipal,
                code);

            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var claimsPrincipal = principal as ClaimsPrincipal;
                if (claimsPrincipal == null)
                {
                    return result;
                }

                _generateAuthorizationResponse.Execute(result, authorizationParameter, claimsPrincipal);
            }

            return result;
        }
    }
}
