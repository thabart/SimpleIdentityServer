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

using Moq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Factories;
using SimpleIdentityServer.Client.Operations;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class RegisterClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IRegistrationClient _registrationClient;

        public RegisterClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Registering_A_Client_Then_No_Exception_Is_Thrown()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var client = await _registrationClient.ResolveAsync(new Core.Common.DTOs.Client
            {
                RedirectUris = new []
                {
                    "https://localhost"
                },
                ScimProfile = true
            }, baseUrl + "/.well-known/openid-configuration");

            // ASSERT
            Assert.NotNull(client);
            Assert.True(client.ScimProfile);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _registrationClient = new RegistrationClient(new RegisterClientOperation(_httpClientFactoryStub.Object), new GetDiscoveryOperation(_httpClientFactoryStub.Object));
        }
    }
}
