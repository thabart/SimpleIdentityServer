namespace SimpleBus.Core
{
    public interface IEventPublisher
    {
        void Publish<T>(T evt) where T : Event;
    }
}
