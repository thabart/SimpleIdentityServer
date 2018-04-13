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
using SimpleIdentityServer.Core.Common.Serializers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseModes
    {
        [EnumMember(Value = ResponseModeNames.None)]
        None,
        [EnumMember(Value = ResponseModeNames.Query)]
        Query,
        [EnumMember(Value = ResponseModeNames.Fragment)]
        Fragment,
        [EnumMember(Value = ResponseModeNames.FormPost)]
        FormPost
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseTypes
    {
        [EnumMember(Value = ResponseTypeNames.Code)]
        Code,
        [EnumMember(Value = ResponseTypeNames.Token)]
        Token,
        [EnumMember(Value = ResponseTypeNames.IdToken)]
        IdToken
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DisplayModes
    {
        [EnumMember(Value = PageNames.Page)]
        Page,
        [EnumMember(Value = PageNames.Popup)]
        Popup,
        [EnumMember(Value = PageNames.Touch)]
        Touch,
        [EnumMember(Value = PageNames.Wap)]
        Wap
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CodeChallengeMethods
    {
        [EnumMember(Value = CodeChallenges.Plain)]
        Plain,
        [EnumMember(Value = CodeChallenges.S256)]
        S256
    }

    [DataContract]
    public class AuthorizationRequest
    {
        private Dictionary<ResponseTypes, string> _mappingResponseTypesToNames = new Dictionary<ResponseTypes, string>
        {
            { ResponseTypes.Code, ResponseTypeNames.Code },
            { ResponseTypes.Token, ResponseTypeNames.Token },
            { ResponseTypes.IdToken, ResponseTypeNames.IdToken }
        };

        public AuthorizationRequest() { }

        public AuthorizationRequest(IEnumerable<string> scopes, IEnumerable<ResponseTypes> responseTypes, string clientId, string redirectUri, string state)
        {
            Scope = string.Join(" ", scopes);
            ResponseType = string.Join(" ", responseTypes.Select(s => _mappingResponseTypesToNames[s]));
            ClientId = clientId;
            RedirectUri = redirectUri;
            State = state;
        }

        [DataMember(Name = RequestAuthorizationCodeNames.Scope)]
        public string Scope { get; set; }
        [DataMember(Name = RequestAuthorizationCodeNames.ResponseType)]
        public string ResponseType { get; set; }
        [DataMember(Name = RequestAuthorizationCodeNames.RedirectUri)]
        public string RedirectUri { get; set; }
        [DataMember(Name = RequestAuthorizationCodeNames.State)]
        public string State { get; set; }
        [DataMember(Name = RequestAuthorizationCodeNames.ResponseMode)]
        public ResponseModes? ResponseMode { get; set; }
        [DataMember(Name = RequestAuthorizationCodeNames.Nonce)]
        public string Nonce { get; set; }
        [DataMember(Name = RequestAuthorizationCodeNames.Display)]
        public DisplayModes? Display { get; set; }
        /// <summary>
        /// The possible values are : none, login, consent, select_account
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.Prompt)]
        public string Prompt { get; set; }
        /// <summary>
        /// Maximum authentication age.
        /// Specifies allowable elapsed time in seconds since the last time the end-user
        ///  was actively authenticated by the OP.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.MaxAge)]
        public double MaxAge { get; set; }
        /// <summary>
        /// End-User's preferred languages
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.UiLocales)]
        public string UiLocales { get; set; }
        /// <summary>
        /// Token previousely issued by the Authorization Server.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.IdTokenHint)]
        public string IdTokenHint { get; set; }
        /// <summary>
        /// Hint to the authorization server about the login identifier the end-user might use to log in.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.LoginHint)]
        public string LoginHint { get; set; }
        /// <summary>
        /// Request that specific Claims be returned from the UserInfo endpoint and/or in the id token.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.Claims)]
        public string Claims { get; set; }
        /// <summary>
        /// Requested Authentication Context Class References values.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.AcrValues)]
        public string AcrValues { get; set; }
        /// <summary>
        /// Self-contained parameter and can be optionally be signed and / or encrypted
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.Request)]
        public string Request { get; set; }
        /// <summary>
        /// Enables OpenID connect requests to be passed by reference rather than by value.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.RequestUri)]
        public string RequestUri { get; set; }
        /// <summary>
        /// Code challenge.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.CodeChallenge)]
        public string CodeChallenge { get; set; }
        /// <summary>
        /// Code challenge method.
        /// </summary>
        [DataMember(Name = RequestAuthorizationCodeNames.CodeChallengeMethod)]
        public CodeChallengeMethods? CodeChallengeMethod { get; set; }
        [DataMember(Name = ClientAuthNames.ClientId)]
        public string ClientId { get; set; }
        [DataMember(Name = EventResponseNames.AggregateId)]
        public string ProcessId { get; set; }
        [DataMember(Name = "origin_url")]
        public string OriginUrl { get; set; }
        [DataMember(Name = "session_id")]
        public string SessionId { get; set; }
        public string GetQueryString()
        {
            var serializer = new ParamSerializer();
            return serializer.Serialize(this);
        }
    }
}
