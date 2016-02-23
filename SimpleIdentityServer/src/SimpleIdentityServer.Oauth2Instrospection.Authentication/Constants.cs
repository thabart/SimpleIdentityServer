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

namespace SimpleIdentityServer.Oauth2Instrospection.Authentication
{
    public static class Constants
    {
        public static class IntrospectionResponseNames
        {
            public const string Active = "active";
            public const string Scope = "scope";
            public const string ClientId = "client_id";
            public const string UserName = "username";
            public const string TokenType = "token_type";
            public const string Expiration = "exp";
            public const string IssuedAt = "iat";
            public const string Nbf = "nbf";
            public const string Subject = "sub";
            public const string Audience = "aud";
            public const string Issuer = "iss";
            public const string Jti = "jti";
        }

        public static class IntrospectionRequestNames
        {
            public const string Token = "token";
            public const string TokenTypeHint = "token_type_hint";
            public const string ClientId = "client_id";
            public const string ClientSecret = "client_secret";
            public const string ClientAssertion = "client_assertion";
            public const string ClientAssertionType = "client_assertion_type";
        }

        public static class ClaimNames
        {
            public const string Scope = "scope";
            public const string Subject = "sub";
        }
    }
}
