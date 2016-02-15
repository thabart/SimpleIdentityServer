using System;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin.Security;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Parsers;
using SimpleIdentityServer.Core.Results;
using ActionResult = System.Web.Mvc.ActionResult;
using ResponseMode = SimpleIdentityServer.Core.Parameters.ResponseMode;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns the authenticated user from an ASP.NET MVC controller.
        /// </summary>
        /// <param name="controller">ASP.NET MVC controller</param>
        /// <returns>Authenticated user</returns>
        public static ClaimsPrincipal GetAuthenticatedUser(this Controller controller)
        {
            var authentication = controller.Request.GetOwinContext().Authentication;
            return authentication.User;
        }

        /// <summary>
        /// Returns the authentication manager from an ASP.NET MVC controller
        /// </summary>
        /// <param name="controller">ASP.NET MVC controller</param>
        /// <returns>Authentication manager</returns>
        public static IAuthenticationManager GetAuthenticationManager(this Controller controller)
        {
            return controller.Request.GetOwinContext().Authentication;
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
    }
}