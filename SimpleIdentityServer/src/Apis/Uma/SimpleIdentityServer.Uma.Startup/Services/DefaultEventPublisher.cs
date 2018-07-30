using SimpleBus.Core;

namespace SimpleIdentityServer.Uma.Startup.Services
{
    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
