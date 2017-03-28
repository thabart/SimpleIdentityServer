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

using System;
using SimpleIdentityServer.Core.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Handlers
{
    public class Payload
    {
        public string ClientId { get; set; }
        public object Content { get; set; }
    }

    public interface IPayloadSerializer
    {
        string GetPayload(AuthorizationRequestReceived parameter);
        string GetPayload(AuthorizationGranted parameter);
        string GetPayload(ResourceOwnerAuthenticated parameter);
        string GetPayload(ConsentAccepted parameter);
        string GetPayload(ActionResult parameter);
    }

    public class PayloadSerializer : IPayloadSerializer
    {
        public string GetPayload(AuthorizationRequestReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter.Parameter));
            }

            var param = parameter.Parameter;
            var payload = new Payload
            {
                ClientId = param.ClientId,
                Content = param
            };
            return JsonConvert.SerializeObject(payload);
        }

        public string GetPayload(AuthorizationGranted parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return GetPayload(parameter.Parameter);
        }

        public string GetPayload(ResourceOwnerAuthenticated parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return GetPayload(parameter.Parameter);
        }

        public string GetPayload(ConsentAccepted parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return GetPayload(parameter.Parameter);
        }

        public string GetPayload(ActionResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var jsonObj = new JObject();
            jsonObj.Add("action_type", Enum.GetName(typeof(TypeActionResult), parameter.Type).ToLower());
            if (parameter.RedirectInstruction != null)
            {
                var redirectInstr = parameter.RedirectInstruction;
                jsonObj.Add("endpoint", Enum.GetName(typeof(IdentityServerEndPoints), redirectInstr.Action));
                jsonObj.Add("response_mode", Enum.GetName(typeof(ResponseMode), redirectInstr.ResponseMode).ToLower());
                var arr = new JArray();
                if (redirectInstr.Parameters != null)
                {
                    foreach (var p in redirectInstr.Parameters)
                    {
                        arr.Add(new JObject(new JProperty(p.Name, p.Value)));
                    }
                }

                jsonObj.Add("parameters", arr);
            }

            var result = new Payload
            {
                Content = jsonObj
            };
            return JsonConvert.SerializeObject(result);
        }
    }
}
