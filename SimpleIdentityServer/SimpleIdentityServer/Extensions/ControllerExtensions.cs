using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

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
    }
}