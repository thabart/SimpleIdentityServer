using SimpleBus.Core;
using System.Threading.Tasks;

namespace SimpleBus.InMemory
{
    public class InMemoryBus : IEventPublisher
    {
        private readonly IEvtHandlerStore _evtHandlerStore;

        public InMemoryBus(IEvtHandlerStore evtHandlerStore)
        {
            _evtHandlerStore = evtHandlerStore;
        }

        public void Publish<T>(T evt) where T : Event
        {
            var handlers = _evtHandlerStore.Get<T>();
            if (handlers == null)
            {
                return;
            }

            foreach (var handler in handlers)
            {
                var handler1 = handler;
                Task.Run(() => handler1.Handle(evt));
            }
        }
    }
}
