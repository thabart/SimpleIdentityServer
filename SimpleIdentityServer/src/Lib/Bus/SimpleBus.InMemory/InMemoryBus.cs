using SimpleBus.Core;
using System.Threading.Tasks;

namespace SimpleBus.InMemory
{
    internal sealed class InMemoryBus : IEventPublisher
    {
        private readonly IEvtHandlerStore _evtHandlerStore;
        private readonly SimpleBusOptions _simpleBusOptions;

        public InMemoryBus(IEvtHandlerStore evtHandlerStore, SimpleBusOptions simpleBusOptions)
        {
            _evtHandlerStore = evtHandlerStore;
            _simpleBusOptions = simpleBusOptions;
        }

        public void Publish<T>(T evt) where T : Event
        {
            var handlers = _evtHandlerStore.Get<T>();
            if (handlers == null)
            {
                return;
            }

            evt.ServerName = _simpleBusOptions.ServerName;
            foreach (var handler in handlers)
            {
                var handler1 = handler;
                Task.Run(() => handler1.Handle(evt));
            }
        }
    }
}
