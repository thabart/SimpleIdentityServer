using System.Security.Claims;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        /// <summary>
        /// Returns if the user is authenticated
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>The user is authenticated</returns>
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return principal.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns the subject from an authenticated user
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>User's subject</returns>
        public static string GetSubject(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
