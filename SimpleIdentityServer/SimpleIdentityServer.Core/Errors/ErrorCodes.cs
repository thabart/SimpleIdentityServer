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
namespace SimpleIdentityServer.Core.Errors
{
    public static class ErrorCodes
    {
        public static string UnhandledExceptionCode = "unhandled_exception";

        #region Standard error codes

        public static string InvalidRequestCode = "invalid_request";

        public static string InvalidClient = "invalid_client";

        public static string InvalidGrant = "invalid_grant";

        public static string InvalidToken = "invalid_token";

        public static string UnAuthorizedClient = "unauthorized_client";

        public static string UnSupportedGrantType = "unsupported_grant_type";

        public static string InvalidScope = "invalid_scope";

        public static string InvalidRequestUriCode = "invalid_request_uri";

        public static string LoginRequiredCode = "login_required";

        public static string InteractionRequiredCode = "interaction_required";

        #endregion
    }
}
