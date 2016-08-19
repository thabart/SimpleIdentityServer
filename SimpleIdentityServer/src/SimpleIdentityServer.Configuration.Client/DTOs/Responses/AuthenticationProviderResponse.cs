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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Configuration.Client.DTOs.Responses
{
    public enum AuthenticationProviderResponseTypes : int
    {
        OAUTH2 = 1,
        OPENID = 2
    }

    [DataContract]
    public class AuthenticationProviderResponse
    {
        [DataMember(Name = Constants.AuthProviderResponseNames.Name)]
        public string Name { get; set; }

        [DataMember(Name = Constants.AuthProviderResponseNames.IsEnabled)]
        public bool IsEnabled { get; set; }

        // 1 : OAUTH2
        [DataMember(Name = Constants.AuthProviderResponseNames.Type)]
        public AuthenticationProviderResponseTypes Type { get; set; }

        [DataMember(Name = Constants.AuthProviderResponseNames.CallbackPath)]
        public string CallbackPath { get; set; }

        [DataMember(Name = Constants.AuthProviderResponseNames.Code)]
        public string Code { get; set; }

        [DataMember(Name = Constants.AuthProviderResponseNames.ClassName)]
        public string ClassName { get; set; }

        [DataMember(Name = Constants.AuthProviderResponseNames.Namespace)]
        public string Namespace { get; set; }

        [DataMember(Name = Constants.AuthProviderResponseNames.Options)]
        public List<Option> Options { get; set; }
    }
}
