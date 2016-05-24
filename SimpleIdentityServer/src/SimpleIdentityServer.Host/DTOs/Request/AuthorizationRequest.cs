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

namespace SimpleIdentityServer.Host.DTOs.Request
{
    public enum ResponseMode
    {
        None,
        query,
        fragment,
        form_post
    }

    public enum ResponseType
    {
        code,
        token,
        id_token
    }

    public enum Display
    {
        page,
        popup,
        touch,
        wap
    }

    public class AuthorizationRequest
    {
        public string scope { get; set; }

        public string response_type { get; set; }

        public string client_id { get; set; }

        public string redirect_uri { get; set; }

        public string state { get; set; }

        public ResponseMode response_mode { get; set; }

        public string nonce { get; set; }

        public Display display { get; set; }

        /// <summary>
        /// The possible values are : none, login, consent, select_account
        /// </summary>
        public string prompt { get; set; }

        /// <summary>
        /// Maximum authentication age.
        /// Specifies allowable elapsed time in seconds since the last time the end-user
       ///  was actively authenticated by the OP.
        /// </summary>
        public double max_age { get; set; }

        /// <summary>
        /// End-User's preferred languages
        /// </summary>
        public string ui_locales { get; set; }

        /// <summary>
        /// Token previousely issued by the Authorization Server.
        /// </summary>
        public string id_token_hint { get; set; }

        /// <summary>
        /// Hint to the authorization server about the login identifier the end-user might use to log in.
        /// </summary>
        public string login_hint { get; set; }

        /// <summary>
        /// Request that specific Claims be returned from the UserInfo endpoint and/or in the id token.
        /// </summary>
        public string claims { get; set; }

        /// <summary>
        /// Requested Authentication Context Class References values.
        /// </summary>
        public string acr_values { get; set; }

        /// <summary>
        /// Self-contained parameter and can be optionally be signed and / or encrypted
        /// </summary>
        public string request { get; set; }

        /// <summary>
        /// Enables OpenID connect requests to be passed by reference rather than by value.
        /// </summary>
        public string request_uri { get; set; }
    }
}