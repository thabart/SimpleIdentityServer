using System.Security.Principal;

using System.Web.Http;

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
    }
}