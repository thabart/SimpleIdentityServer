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
using SimpleIdentityServer.UserInformation.Authentication.Tests.Fake;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using SimpleIdentityServer.UserInformation.Authentication.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;

namespace SimpleIdentityServer.UserInformation.Authentication.Tests
{
    public class UserInformationMiddlewareFixture
    {
        private class FakeStartup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddMvc();
            }

            public void Configure(IApplicationBuilder app)
            {
                var options = app.ApplicationServices.GetRequiredService<UserInformationOptions>();
                app.UseAuthenticationWithUserInformation(options);
                app.Map("/protectedoperation", a =>
                {
                    a.Run(async context =>
                    {
                        var user = context.User;
                        var claim = user.Claims.ToList().FirstOrDefault(c => c.Type == "role");
                        if (claim == null || claim.Value != "administrator")
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
        public async Task When_Passing_NotWellFormed_UserInformationEndPoint_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var userInformationResponse = new Dictionary<string, string>
            {
                { "role", "administrator" }
            };
            var json = JsonConvert.SerializeObject(userInformationResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new UserInformationOptions
            {
                UserInformationEndPoint = "invalid_url",
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
            Assert.True(exception.Message == ErrorDescriptions.TheUserInfoEndPointIsNotAWellFormedUrl);
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Passing_An_Access_Token_Valid_For_The_Role_Administrator_Then_Request_Is_Authorized()
        {
            // ARRANGE
            var userInformationResponse = new Dictionary<string, string>
            {
                { "role", "administrator" }
            };
            var json = JsonConvert.SerializeObject(userInformationResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new UserInformationOptions
            {
                UserInformationEndPoint = "http://localhost:5000/userinfo",
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
        public async Task When_Passing_An_Access_Token_Not_Valid_For_The_Role_Then_Request_Is_Not_Authorized()
        {
            // ARRANGE
            var userInformationResponse = new Dictionary<string, string>
            {
                { "role", "invalid_role" }
            };
            var json = JsonConvert.SerializeObject(userInformationResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fakeHttpHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var options = new UserInformationOptions
            {
                UserInformationEndPoint = "http://localhost:5000/userinfo",
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

        private static TestServer CreateServer(UserInformationOptions options)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices((services) =>
                {
                    InitializeServices(services, options);
                })
                .UseStartup(typeof(FakeStartup));
            return new TestServer(builder);
        }
        private static void InitializeServices(IServiceCollection services, UserInformationOptions options)
        {
            services.AddSingleton(options);
        }
    }
}
