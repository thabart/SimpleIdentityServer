namespace SimpleIdServer.Bus
{
    public interface IEventHandler
    {

    }

    public interface IEventHandler<T>  : IEventHandler where T : Event
    {
        void Handle(T evt);
    }
}