using Microsoft.Extensions.OptionsModel;

namespace SimpleIdentityServer.Oauth2Instrospection.Authentication.Options
{
    public static class Options
    {
        public static IOptions<TOptions> Create<TOptions>(TOptions options) where TOptions : class, new()
        {
            return new OptionsWrapper<TOptions>(options);
        }
    }
}
