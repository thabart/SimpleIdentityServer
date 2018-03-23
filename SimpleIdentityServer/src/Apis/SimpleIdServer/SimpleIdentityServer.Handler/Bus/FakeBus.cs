#region copyright
// Copyright 2017 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Threading.Tasks;


namespace SimpleIdentityServer.Handler.Bus
{
    public interface IEventPublisher
    {
        void Publish<T>(T evt) where T : Event;
    }

    public class FakeBus : IEventPublisher
    {
        private readonly IEvtHandlerStore _evtHandlerStore;

        public FakeBus(IEvtHandlerStore evtHandlerStore)
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

    public interface IHandler
    {

    }

    public interface IHandle<T> : IHandler
    {
        Task Handle(T message);
    }
}
