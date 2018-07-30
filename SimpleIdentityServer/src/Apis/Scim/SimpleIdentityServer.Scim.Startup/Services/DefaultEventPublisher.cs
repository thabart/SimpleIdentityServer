using SimpleBus.Core;

namespace SimpleIdentityServer.Scim.Startup.Services
{
    internal sealed class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
