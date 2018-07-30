namespace SimpleBus.Core
{
    public interface IEventHandler
    {

    }

    public interface IEventHandler<T>  : IEventHandler where T : Event
    {
        void Handle(T evt);
    }
}