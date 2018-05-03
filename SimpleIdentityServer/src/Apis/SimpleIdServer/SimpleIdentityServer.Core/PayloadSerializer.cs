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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Core
{
    public class Payload
    {
        public string ClientId { get; set; }
        public object Authorization { get; set; }
        public object Content { get; set; }
    }

    public interface IPayloadSerializer
    {
        string GetPayload(AuthorizationParameter parameter);
        string GetPayload(IntrospectionParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(IntrospectionResult parameter);
        string GetPayload(RegistrationParameter parameter);
        string GetPayload(ClientRegistrationResponse parameter);
        string GetPayload(string accessToken);
        string GetPayload(UserInfoResult parameter);
        string GetPayload(AuthorizationCodeGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(ClientCredentialsGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(RefreshTokenGrantTypeParameter parameter);
        string GetPayload(ResourceOwnerGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(RevokeTokenParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(GrantedToken parameter);
        string GetPayload(Results.ActionResult parameter);
    }

    public class PayloadSerializer : IPayloadSerializer
    {
        public string GetPayload(AuthorizationParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return JsonConvert.SerializeObject(new Payload
            {
                ClientId = parameter.ClientId,
                Content = parameter
            });
        }

        public string GetPayload(IntrospectionParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var jsonObj = new JObject();
            jsonObj.Add("token", parameter.Token);
            jsonObj.Add("token_type_hint", parameter.Token);
            jsonObj.Add("client_id", parameter.ClientId);
            jsonObj.Add("client_secret", parameter.ClientSecret);
            jsonObj.Add("client_assertion", parameter.ClientAssertion);
            jsonObj.Add("client_assertion_type", parameter.ClientAssertionType);
            var clientId = GetClientId(authenticationHeaderValue);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = parameter.ClientId;
            }

            var result = new Payload
            {
                Authorization = authenticationHeaderValue,
                Content = jsonObj,
                ClientId = clientId
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(IntrospectionResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = new Payload
            {
                Content = parameter
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(RegistrationParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var result = new Payload
            {
                Content = parameter
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(ClientRegistrationResponse parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var result = new Payload
            {
                ClientId = parameter.ClientId,
                Content = parameter
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var jObj = new JObject();
            jObj.Add("access_token", accessToken);
            var result = new Payload
            {
                Content = jObj
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(UserInfoResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = new Payload
            {
                Content = ((ObjectResult)parameter.Content).Value
            };
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(AuthorizationCodeGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var clientId = GetClientId(authenticationHeaderValue);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = parameter.ClientId;
            }

            var result = new Payload
            {
                ClientId = clientId,
                Authorization = BuildAuthHeader(authenticationHeaderValue),
                Content = parameter
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(ClientCredentialsGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var clientId = GetClientId(authenticationHeaderValue);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = parameter.ClientId;
            }

            var result = new Payload
            {
                ClientId = clientId,
                Authorization = BuildAuthHeader(authenticationHeaderValue),
                Content = parameter
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(RefreshTokenGrantTypeParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var result = new Payload
            {
                Content = parameter
            };
            
            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(ResourceOwnerGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var clientId = GetClientId(authenticationHeaderValue);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = parameter.ClientId;
            }

            var result = new Payload
            {
                ClientId = clientId,
                Authorization = BuildAuthHeader(authenticationHeaderValue),
                Content = parameter
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(RevokeTokenParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var clientId = GetClientId(authenticationHeaderValue);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = parameter.ClientId;
            }

            var result = new Payload
            {
                ClientId = clientId,
                Authorization = BuildAuthHeader(authenticationHeaderValue),
                Content = parameter
            };

            return JsonConvert.SerializeObject(result);
        }

        public string GetPayload(GrantedToken parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            var result = new Payload
            {
                Content = parameter
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
