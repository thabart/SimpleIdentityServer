using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace WsFederation
{
    public class WsFedAuthenticationMiddleware : AuthenticationMiddleware<WsFedAuthenticationOptions>
    {
        #region Constructor

        public WsFedAuthenticationMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            IOptions<WsFedAuthenticationOptions> options
            ) : base(next, options, loggerFactory, urlEncoder)
        {
        }

        #endregion

        #region Protected methods

        protected override AuthenticationHandler<WsFedAuthenticationOptions> CreateHandler()
        {
            return new WsFedAuthenticationHandler();
        }

        #endregion
    }
}
