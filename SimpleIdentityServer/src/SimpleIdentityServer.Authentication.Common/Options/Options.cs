using Microsoft.Extensions.OptionsModel;

namespace SimpleIdentityServer.Authentication.Common.Options
{
    public static class Options
    {
        public static IOptions<TOptions> Create<TOptions>(TOptions options) where TOptions : class, new()
        {
            return new OptionsWrapper<TOptions>(options);
        }
    }
}
