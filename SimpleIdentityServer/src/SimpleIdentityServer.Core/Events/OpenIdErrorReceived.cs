﻿#region copyright
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

using SimpleIdentityServer.Core.Bus;

namespace SimpleIdentityServer.Core.Events
{
    public class OpenIdErrorReceived : Event
    {
        public OpenIdErrorReceived(string id, string processId, string code, string message, int order, string state = null)
        {
            Id = id;
            ProcessId = processId;
            Code = code;
            Message = message;
            State = state;
            Order = order;
        }

        public string Id { get; private set; }
        public string ProcessId { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string State { get; private set; }
        public int Order { get; private set; }
    }
}
