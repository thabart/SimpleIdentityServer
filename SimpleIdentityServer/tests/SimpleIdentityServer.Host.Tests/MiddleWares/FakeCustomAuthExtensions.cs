#region copyright
// Copyright 2016 Habart Thierry
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

using Microsoft.AspNetCore.Authentication;
using System;

namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    public static class FakeCustomAuthExtensions
    {
        public static AuthenticationBuilder AddFakeCustomAuth(this AuthenticationBuilder builder, Action<TestAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(FakeStartup.DefaultSchema, FakeStartup.DefaultSchema, configureOptions);
        }

        public static AuthenticationBuilder AddFakeOAuth2Introspection(this AuthenticationBuilder builder, Action<FakeOAuth2IntrospectionOptions> configureOptions)
        {
            return builder.AddScheme<FakeOAuth2IntrospectionOptions, FakeOauth2IntrospectionHandler>(FakeOAuth2IntrospectionOptions.AuthenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddFakeUserInfoIntrospection(this AuthenticationBuilder builder, Action<FakeUserInfoIntrospectionOptions> configureOptions)
        {
            return builder.AddScheme<FakeUserInfoIntrospectionOptions, FakeUserInfoIntrospectionHandler>(FakeUserInfoIntrospectionOptions.AuthenticationScheme, configureOptions);
        }
    }
}
