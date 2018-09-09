using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class AuthenticateServiceExtensions
    {
        public static async Task<ClaimsPrincipal> GetAuthenticatedUser(this IAuthenticationService authenticateService, Controller controller, string scheme)
        {
            if (authenticateService == null)
            {
                throw new ArgumentNullException(nameof(authenticateService));
            }

            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentNullException(scheme);
            }

            var authResult = await authenticateService.AuthenticateAsync(controller.HttpContext, scheme).ConfigureAwait(false);
            if (authResult == null || authResult.Principal == null)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            return authResult.Principal;
        }
    }
}
