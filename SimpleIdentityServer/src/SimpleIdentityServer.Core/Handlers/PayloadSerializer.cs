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
using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Common.Extensions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.Core.Handlers
{
    public class Payload
    {
        public string ClientId { get; set; }
        public object Authorization { get; set; }
        public object Content { get; set; }
    }

    public interface IPayloadSerializer
    {
        string GetPayload(AuthorizationRequestReceived parameter);
        string GetPayload(AuthorizationGranted parameter);
        string GetPayload(ResourceOwnerAuthenticated parameter);
        string GetPayload(ConsentAccepted parameter);
        string GetPayload(IntrospectionRequestReceived parameter);
        string GetPayload(RegistrationResultReceived parameter);
        string GetPayload(RegistrationReceived parameter);
        string GetPayload(GetUserInformationReceived parameter);
        string GetPayload(UserInformationReturned parameter);
        string GetPayload(Results.ActionResult parameter);
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

        public string GetPayload(IntrospectionRequestReceived parameter)
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
            var jsonObj = new JObject();
            jsonObj.Add("token", param.Token);
            jsonObj.Add("token_type_hint", param.Token);
            jsonObj.Add("client_id", param.ClientId);
            jsonObj.Add("client_secret", param.ClientSecret);
            jsonObj.Add("client_assertion", param.ClientAssertion);
            jsonObj.Add("client_assertion_type", param.ClientAssertionType);
            var clientId = GetClientId(parameter.AuthenticationHeader);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = param.ClientId;
            }

            var result = new Payload
            {
                Authorization = BuildAuthHeader(parameter.AuthenticationHeader),
                Content = jsonObj,
                ClientId = clientId
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(IntrospectionResultReturned parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter.Parameter));
            }

            var result = new Payload
            {
                Content = parameter.Parameter
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(RegistrationReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter.Parameter));
            }

            var result = new Payload
            {
                Content = parameter.Parameter
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(RegistrationResultReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter.Parameter));
            }

            var result = new Payload
            {
                ClientId = parameter.Parameter.ClientId,
                Content = parameter.Parameter
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(GetUserInformationReceived parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Parameter))
            {
                throw new ArgumentNullException(nameof(parameter.Parameter));
            }

            var jObj = new JObject();
            jObj.Add("access_token", parameter.Parameter);
            var result = new Payload
            {
                Content = jObj
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(UserInformationReturned parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter.Parameter));
            }

            var result = new Payload
            {
                Content = ((ObjectResult)parameter.Parameter.Content).Value
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(Results.ActionResult parameter)
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

        private JObject BuildAuthHeader(AuthenticationHeaderValue auth)
        {
            if (auth == null)
            {
                return null;
            }

            var result = new JObject();
            result.Add("scheme", auth.Scheme);
            result.Add("value", auth.Parameter);
            return result; 
        }

        private static string GetClientId(AuthenticationHeaderValue auth)
        {
            if (auth == null || string.IsNullOrWhiteSpace(auth.Parameter))
            {
                return null;
            }

            try
            {
                var decodedParameter = auth.Parameter.Base64Decode();
                var splitted = decodedParameter.Split(':');
                if (splitted == null || splitted.Count() != 2)
                {
                    return null;
                }

                return splitted.First();
            }
            catch
            {
                return null;
            }
        }
    }
}
