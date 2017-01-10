using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RpConformance.Extensions
{
    public static class ControllerExtensions
    {
        public static ClaimsPrincipal GetAuthenticatedUser(this Controller controller)
        {
            return controller.Request.HttpContext.User;
        }

        public static AuthenticationManager GetAuthenticationManager(this Controller controller)
        {
            return controller.HttpContext.Authentication;
        }
    }
}
