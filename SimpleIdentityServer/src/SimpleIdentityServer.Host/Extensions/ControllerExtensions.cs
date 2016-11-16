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

using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Parsers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;
using ResponseMode = SimpleIdentityServer.Core.Parameters.ResponseMode;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<ClaimsPrincipal> GetAuthenticatedUser(this Controller controller, string scheme)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentNullException(scheme);
            }

            var user = await controller.HttpContext.Authentication.AuthenticateAsync(scheme);
            return user ?? new ClaimsPrincipal(new ClaimsIdentity());
        }

        public static async Task<ClaimsPrincipal> GetAuthenticatedUser2F(this Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            var user  = await controller.HttpContext.Authentication.AuthenticateAsync(Constants.TwoFactorCookieName);
            return user ?? new ClaimsPrincipal(new ClaimsIdentity());
        }

        public static AuthenticationManager GetAuthenticationManager(this Controller controller) 
        {
            return controller.HttpContext.Authentication;
        }

        public static ActionResult CreateRedirectionFromActionResult(
            this Controller controller,
            Core.Results.ActionResult actionResult,
            AuthorizationRequest authorizationRequest)
        {
            var actionResultParser = ActionResultParserFactory.CreateActionResultParser();
            if (actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var parameters = actionResultParser.GetRedirectionParameters(actionResult);
                var uri = new Uri(authorizationRequest.redirect_uri);
                var redirectUrl = controller.CreateRedirectHttp(uri, parameters, actionResult.RedirectInstruction.ResponseMode);
                return new RedirectResult(redirectUrl);
            }

            var actionInformation =
                actionResultParser.GetControllerAndActionFromRedirectionActionResult(actionResult);
            if (actionInformation != null)
            {
                var routeValueDic = actionInformation.RouteValueDictionary;
                routeValueDic.Add("controller", actionInformation.ControllerName);
                routeValueDic.Add("action", actionInformation.ActionName);
                return new RedirectToRouteResult(routeValueDic);
            }

            return null;
        }
        
        public static RedirectResult CreateRedirectHttpTokenResponse(
            this Controller controller,
            Uri uri,
            RouteValueDictionary parameters,
            ResponseMode responseMode)
        {
            switch (responseMode)
            {
                case ResponseMode.fragment:
                    uri = uri.AddParametersInFragment(parameters);
                break;
                case ResponseMode.query:
                    uri = uri.AddParametersInQuery(parameters);
                    break;
            }

            return new RedirectResult(uri.AbsoluteUri);
        }

        /// <summary>
        /// Create a redirection HTTP response message based on the response mode.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <param name="responseMode"></param>
        /// <returns></returns>
        public static string CreateRedirectHttp(
            this Controller controller,
            Uri uri,
            RouteValueDictionary parameters,
            ResponseMode responseMode)
        {
            switch (responseMode)
            {
                case ResponseMode.fragment:
                    uri = uri.AddParametersInFragment(parameters);
                    break;
                default:
                    uri = uri.AddParametersInQuery(parameters);
                    break;
            }

            return uri.ToString();
        }
    }
}