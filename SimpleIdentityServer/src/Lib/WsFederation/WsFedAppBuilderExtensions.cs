using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace WsFederation
{
    public static class WsFedAppBuilderExtensions
    {
        #region Public static methods

        public static IApplicationBuilder UseWsFedAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseWsFedAuthentication(new WsFedAuthenticationOptions());
        }


        public static IApplicationBuilder UseWsFedAuthentication(
            this IApplicationBuilder app, 
            Action<WsFedAuthenticationOptions> configureOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new WsFedAuthenticationOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }

            return app.UseWsFedAuthentication(options);
        }


        public static IApplicationBuilder UseWsFedAuthentication(
            this IApplicationBuilder app, 
            WsFedAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<WsFedAuthenticationMiddleware>(Options.Create(options));
        }

        #endregion
    }
}
