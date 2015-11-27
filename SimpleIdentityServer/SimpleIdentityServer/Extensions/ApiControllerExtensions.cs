using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;

using System.Web.Http;
using System.Web.Routing;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class ApiControllerExtensions
    {        
        /// <summary>
        /// Returns the authenticated user from an ASP.NET API controller.
        /// </summary>
        /// <param name="controller">ASP.NET API controller</param>
        /// <returns>Authenticated user</returns>
        public static IPrincipal GetAuthenticatedUser(this ApiController controller)
        {
            return controller.User;
        }

        /// <summary>
        /// Create a redirection HTTP response message based on the response mode.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <param name="responseMode"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateRedirectHttpTokenResponse(
            this ApiController controller,
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

            return CreateMoveHttpResponse(controller, uri.ToString());
        }

        private static HttpResponseMessage CreateMoveHttpResponse(ApiController controller, string url)
        {
            var response = controller.Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(url);
            return response;
        }
    }
}