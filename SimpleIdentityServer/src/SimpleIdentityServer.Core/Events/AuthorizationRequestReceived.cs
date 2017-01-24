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

using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Events
{
    public class AuthorizationRequestReceived : Event
    {
        private readonly string _id;
        private readonly string _processId;
        private readonly AuthorizationParameter _parameter;

        public AuthorizationRequestReceived(string id, string processId, AuthorizationParameter parameter)
        {
            _id = id;
            _processId = processId;
            _parameter = parameter;
        }
    }
}
