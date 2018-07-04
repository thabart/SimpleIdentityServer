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

using SimpleBus.Core;
using System;
using System.Collections.Generic;

namespace SimpleBus.InMemory
{
    public interface IEvtHandlerStore
    {
        void Register(Type type, IHandle<Event> handler);
        IEnumerable<IHandle<Event>> Get<T>() where T : Event;
    }

    public class EvtHandlerStore : IEvtHandlerStore
    {
        private readonly IDictionary<Type, ICollection<IHandle<Event>>> _types = new Dictionary<Type, ICollection<IHandle<Event>>>();

        public void Register(Type evtType, IHandle<Event> handler)
        {
            if (!_types.ContainsKey(evtType))
            {
                _types.Add(evtType, new List<IHandle<Event>> { handler });
                return;
            }

            _types[evtType].Add(handler);
        }

        public IEnumerable<IHandle<Event>> Get<T>() where T : Event
        {
            if (!_types.ContainsKey(typeof(T)))
            {
                return new List<IHandle<Event>>();
            }

            return _types[typeof(T)];
        }
    }
}
