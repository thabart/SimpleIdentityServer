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

using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Client.Operations;
using System.Threading.Tasks;
using Xunit;
using SimpleIdentityServer.Core.Common.DTOs;

namespace SimpleIdentityServer.Host.Tests
{
    public class AuthorizationClientFixture : IClassFixture<TestScimServerFixture>
    {
        private readonly TestScimServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IAuthorizationClient _authorizationClient;

        public AuthorizationClientFixture(TestScimServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Requesting_AuthorizationCode_And_RedirectUri_IsNotValid_Then_Error_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration", new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "implicit_client", "http://localhost:5000/invalid_callback", "state"));

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.True(result.Content["error"].ToString() == "invalid_request");
        }

        [Fact]
        public async Task When_Requesting_AuthorizationCode_Then_Code_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration", 
                new AuthorizationRequest(new [] { "openid", "api1" }, new[] { ResponseTypes.Code }, "implicit_client", "http://localhost:5000/callback", "state"));

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotNull(result.Location);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var getAuthorizationOperation = new GetAuthorizationOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            _authorizationClient = new AuthorizationClient(getAuthorizationOperation, getDiscoveryOperation);
        }
    }
}
