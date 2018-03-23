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

using SimpleIdentityServer.Handler.Bus;

namespace SimpleIdentityServer.Handler.Events
{
    public class AuthorizationRequestReceived : Event
    {
        private readonly string _id;
        private readonly string _processId;
        private readonly string _payload;

        public AuthorizationRequestReceived(string id, string processId, string payload, int order)
        {
            _id = id;
            _processId = processId;
            _payload = payload;
            Order = order;
        }

        public string Id
        {
            get { return _id; }
        }

        public string ProcessId 
        {
            get { return _processId; }
        }

        public  string Payload
        {
            get { return _payload; }
        }

        public int Order { get; private set; }
    }
}
