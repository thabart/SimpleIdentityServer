#region copyright
// Copyright 2015 Habart Thierry
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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TwoFactorAuthentications
    {
        [JsonProperty(Constants.TwoFactorAuthenticationNames.None)]
        [EnumMember(Value = Constants.TwoFactorAuthenticationNames.None)]
        None,
        [JsonProperty(Constants.TwoFactorAuthenticationNames.Email)]
        [EnumMember(Value = Constants.TwoFactorAuthenticationNames.Email)]
        Email,
        [JsonProperty(Constants.TwoFactorAuthenticationNames.Sms)]
        [EnumMember(Value = Constants.TwoFactorAuthenticationNames.Sms)]
        Sms
    }

    [DataContract]
    public class ResourceOwnerResponse
    {
        [JsonProperty(Constants.ResourceOwnerResponseNames.Login)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.Login)]
        public string Login { get; set; }

        [JsonProperty(Constants.ResourceOwnerResponseNames.Password)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.Password)]
        public string Password { get; set; }

        [JsonProperty(Constants.ResourceOwnerResponseNames.IsLocalAccount)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.IsLocalAccount)]
        public bool IsLocalAccount { get; set; }

        [JsonProperty(Constants.ResourceOwnerResponseNames.TwoFactorAuthentication)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.TwoFactorAuthentication)]
        public TwoFactorAuthentications TwoFactorAuthentication { get; set; }

        [JsonProperty(Constants.ResourceOwnerResponseNames.Claims)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.Claims)]
        public List<KeyValuePair<string, string>> Claims { get; set; }

        [JsonProperty(Constants.ResourceOwnerResponseNames.CreateDateTime)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.CreateDateTime)]
        public DateTime CreateDateTime { get; set; }

        [JsonProperty(Constants.ResourceOwnerResponseNames.UpdateDateTime)]
        [DataMember(Name = Constants.ResourceOwnerResponseNames.UpdateDateTime)]
        public DateTime UpdateDateTime { get; set; }
    }
}
