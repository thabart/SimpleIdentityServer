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

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Newtonsoft.Json;
using SimpleIdentityServer.Oauth2Instrospection.Authentication.Errors;
using SimpleIdentityServer.Oauth2Instrospection.Authentication.Tests.Fake;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdentityServer.Oauth2Instrospection.Authentication.Tests
{
    public class Oauth2IntrospectionMiddlewareFixture
    {
        #region Exceptions

        [Fact]
        public void When_Passing_NotWellFormed_TokenIntrospectionEndPoint_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = "GetMethod"
            };
            var json = JsonConvert.SerializeObject(introspectionResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "invalid_url",
                ClientId = "MyBlog",
                ClientSecret = "MyBlog",
                BackChannelHttpHandler = fakeHttpHandler
            };
            var createServer = CreateServer(options);
            var client = createServer.CreateClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Authorization", "Bearer accessToken");
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.RequestUri = new Uri("http://localhost/protectedoperation");

            // ACT & ASSERTS
            var content = Assert.ThrowsAsync<ArgumentException>(async () => await client.SendAsync(httpRequestMessage).ConfigureAwait(false));
            var exception = content.Result;
            Assert.True(exception.Message == ErrorDescriptions.TheIntrospectionEndPointIsNotAWellFormedUrl);
        }

        [Fact]
        public void When_No_Client_Id_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = "GetMethod"
            };
            var json = JsonConvert.SerializeObject(introspectionResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "http://localhost:5000/introspect"
            };
            var createServer = CreateServer(options);
            var client = createServer.CreateClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Authorization", "Bearer accessToken");
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.RequestUri = new Uri("http://localhost/protectedoperation");

            // ACT & ASSERTS
            var content = Assert.ThrowsAsync<ArgumentException>(async () => await client.SendAsync(httpRequestMessage).ConfigureAwait(false));
            var exception = content.Result;
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterCannotBeEmpty, nameof(options.ClientId)));
        }
        [Fact]
        public void When_No_Client_Secret_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = "GetMethod"
            };
            var json = JsonConvert.SerializeObject(introspectionResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "http://localhost:5000/introspect",
                ClientId = "client_id"
            };
            var createServer = CreateServer(options);
            var client = createServer.CreateClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Authorization", "Bearer accessToken");
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.RequestUri = new Uri("http://localhost/protectedoperation");

            // ACT & ASSERTS
            var content = Assert.ThrowsAsync<ArgumentException>(async () => await client.SendAsync(httpRequestMessage).ConfigureAwait(false));
            var exception = content.Result;
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterCannotBeEmpty, nameof(options.ClientSecret)));
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Passing_An_Access_Token_Valid_For_The_Scope_ValuesGet_Then_Request_Is_Authorized()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = "GetMethod"
            };
            var json = JsonConvert.SerializeObject(introspectionResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "http://localhost:5000/introspect",
                ClientId = "MyBlog",
                ClientSecret = "MyBlog",
                BackChannelHttpHandler = fakeHttpHandler
            };
            var createServer = CreateServer(options);
            var client = createServer.CreateClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Authorization", "Bearer accessToken");
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.RequestUri = new Uri("http://localhost/protectedoperation");

            // ACT
            var result = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // ASSERT
            Assert.True(result.StatusCode == HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task When_Passing_An_Access_Token_Not_Valid_For_The_Scope_Then_Request_Is_Not_Authorized()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = "unauthorized_scope"
            };
            var json = JsonConvert.SerializeObject(introspectionResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new Oauth2IntrospectionOptions
            {
                InstrospectionEndPoint = "http://localhost:5000/introspect",
                ClientId = "MyBlog",
                ClientSecret = "MyBlog",
                BackChannelHttpHandler = fakeHttpHandler
            };
            var createServer = CreateServer(options);
            var client = createServer.CreateClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("Authorization", "Bearer accessToken");
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.RequestUri = new Uri("http://localhost/protectedoperation");

            // ACT
            var result = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // ASSERT
            Assert.True(result.StatusCode == HttpStatusCode.Unauthorized);
        }

        #endregion

        private static TestServer CreateServer(Oauth2IntrospectionOptions options)
        {
            return TestServer.Create(app =>
            {
                app.UseAuthenticationWithIntrospection(options);
                app.Map("/protectedoperation", a =>
                {
                    a.Run(async context =>
                    {
                        var user = context.User;
                        var claim = user.Claims.ToList().FirstOrDefault(c => c.Type == "scope");
                        if (claim == null || claim.Value != "GetMethod")
                        {
                            context.Response.StatusCode = 401;
                            context.Response.Headers.Add("WWW-Authenticate",
                                new[] { "Basic" });
                        }
                        else
                        {
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 200;           
                        }
                    });
                });
            }, services =>
            {
                services.AddMvc();
            });
        }
    }
}
