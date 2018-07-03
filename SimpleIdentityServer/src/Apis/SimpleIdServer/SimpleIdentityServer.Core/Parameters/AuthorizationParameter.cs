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

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Models;

namespace SimpleIdentityServer.Core.Parameters
{
    public enum ResponseMode
    {
        None,
        query,
        fragment,
        form_post
    }

    public enum Display
    {
        page,
        popup,
        touch,
        wap
    }

    [Flags]
    public enum PromptParameter
    {
        none,
        login,
        consent,
        select_account
    }

    public class ClaimParameter
    {
        public string Name { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public bool Essential
        {
            get { return GetBoolean(Constants.StandardClaimParameterValueNames.EssentialName); }
        }

        public string Value
        {
            get { return GetString(Constants.StandardClaimParameterValueNames.ValueName); }
        }

        public string[] @Values
        {
            get { return GetArray(Constants.StandardClaimParameterValueNames.ValuesName); }
        }

        public bool EssentialParameterExist
        {
            get
            {
                return Parameters.Any(p => p.Key == Constants.StandardClaimParameterValueNames.EssentialName);
            }
        }

        public bool ValueParameterExist
        {
            get
            {
                return Parameters.Any(p => p.Key == Constants.StandardClaimParameterValueNames.ValueName);
            }
        }

        public bool ValuesParameterExist
        {
            get
            {
                return Parameters.Any(p => p.Key == Constants.StandardClaimParameterValueNames.ValuesName);
            }
        }

        private bool GetBoolean(string name)
        {
            var value = GetString(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            bool result;
            if (!bool.TryParse(value, out result))
            {
                return false;
            }

            return result;
        }

        private string GetString(string name)
        {
            var keyPair = Parameters.FirstOrDefault(p => p.Key == name);
            if (keyPair.Equals(default(KeyValuePair<string, object>))
                || string.IsNullOrWhiteSpace(keyPair.ToString()))
            {
                return string.Empty;
            }

            return keyPair.Value.ToString();
        }

        private string[] GetArray(string name)
        {
            var keyPair = Parameters.FirstOrDefault(p => p.Key == name);
            if (keyPair.Equals(default(KeyValuePair<string, object>))
                || string.IsNullOrWhiteSpace(keyPair.ToString()))
            {
                return null;
            }

            var value = keyPair.Value;
            if (!value.GetType().IsArray)
            {
                return null;
            }

            var result = (object[])value;
            return result.Select(r => r.ToString()).ToArray();
        }
    }

    public class ClaimsParameter
    {
        public List<ClaimParameter> UserInfo { get; set; }

        public List<ClaimParameter> IdToken { get; set; }

        public override string ToString()
        {
            return this.SerializeWithJavascript();
        }
    }

    public sealed class AuthorizationParameter
    {
        public string ClientId { get; set; }
        public string Scope { get; set; }
        public string AmrValues { get; set; }
        public string ResponseType { get; set; }
        public string RedirectUrl { get; set; }
        public string State { get; set; }
        public ResponseMode ResponseMode { get; set; }
        public string Nonce { get; set; }
        public Display Display { get; set; }
        public string Prompt { get; set; }
        public double MaxAge { get; set; }
        public string UiLocales { get; set; }
        public string IdTokenHint { get; set; }        
        public string LoginHint { get; set; }
        public string AcrValues { get; set; }
        public ClaimsParameter Claims { get; set; }
        public string CodeChallenge { get; set; }
        public CodeChallengeMethods? CodeChallengeMethod { get; set; } 
        public string ProcessId { get; set; }
        public int ProcessOrder { get; set; }
        public string OriginUrl { get; set; }
        public string SessionId { get; set; }
    }
}
