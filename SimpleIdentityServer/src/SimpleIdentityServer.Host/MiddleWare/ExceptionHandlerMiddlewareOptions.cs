using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Host.MiddleWare
{
    public class ExceptionHandlerMiddlewareOptions
    {
        public ISimpleIdentityServerEventSource SimpleIdentityServerEventSource { get; set; }
    }
}
