using SimpleIdServer.Bus;

namespace SimpleIdentityServer.Uma.Host.Tests.Services
{
    internal sealed class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
