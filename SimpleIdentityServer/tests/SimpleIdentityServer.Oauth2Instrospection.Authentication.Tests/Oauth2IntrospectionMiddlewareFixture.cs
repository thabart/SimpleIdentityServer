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
using SimpleIdentityServer.Oauth2Instrospection.Authentication.Errors;
using SimpleIdentityServer.Oauth2Instrospection.Authentication.Tests.Fake;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

namespace SimpleIdentityServer.Oauth2Instrospection.Authentication.Tests
{

    public class Oauth2IntrospectionMiddlewareFixture
    {
        private class FakeStartup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddMvc();
            }

            public void Configure(IApplicationBuilder app)
            {
                var options = app.ApplicationServices.GetRequiredService<Oauth2IntrospectionOptions>();
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
            }
        }

        #region Exceptions

        [Fact]
        public async Task When_Passing_NotWellFormed_TokenIntrospectionEndPoint_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = new List<string> { "GetMethod" }
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await client.SendAsync(httpRequestMessage)).ConfigureAwait(false);
            Assert.True(exception.Message == ErrorDescriptions.TheIntrospectionEndPointIsNotAWellFormedUrl);
        }

        [Fact]
        public async Task When_No_Client_Id_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = new List<string> { "GetMethod" }
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await client.SendAsync(httpRequestMessage)).ConfigureAwait(false);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterCannotBeEmpty, nameof(options.ClientId)));
        }

        [Fact]
        public async Task When_No_Client_Secret_Is_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var introspectionResponse = new IntrospectionResponse
            {
                Active = true,
                Scope = new List<string> { "GetMethod" }
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await client.SendAsync(httpRequestMessage)).ConfigureAwait(false);
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
                Scope = new List<string> {  "GetMethod" }
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
                Scope = new List<string> { "unauthorized_scope" }
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

        #region Private methods

        private static TestServer CreateServer(Oauth2IntrospectionOptions options)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices((services) =>
                {
                    InitializeServices(services, options);
                })
                .UseStartup(typeof(FakeStartup));
            return new TestServer(builder);
        }

        private static void InitializeServices(IServiceCollection services, Oauth2IntrospectionOptions options)
        {
            services.AddSingleton(options);
        }

        #endregion
    }
}
