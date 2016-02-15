using System;
using System.Security.Claims;
using System.Net.Http;
using System.Web.Routing;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.Parsers;
using SimpleIdentityServer.Core.Results;


using ResponseMode = SimpleIdentityServer.Core.Parameters.ResponseMode;
using Microsoft.AspNet.Mvc;

using ActionResult = Microsoft.AspNet.Mvc.ActionResult;
using Microsoft.AspNet.Http.Authentication;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns the authenticated user
        /// </summary>
        /// <param name="controller">Controller</param>
        /// <returns>Authenticated user</returns>
        public static ClaimsPrincipal GetAuthenticatedUser(this Controller controller)
        {
            return controller.Request.HttpContext.User;
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