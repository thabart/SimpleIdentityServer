using SimpleIdServer.Bus;

namespace SimpleIdentityServer.Host.Tests.Services
{
    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
