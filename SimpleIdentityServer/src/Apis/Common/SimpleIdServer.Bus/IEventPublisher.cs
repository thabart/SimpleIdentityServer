namespace SimpleIdServer.Bus
{
    public interface IEventPublisher
    {
        void Publish<T>(T evt) where T : Event;
    }
}
