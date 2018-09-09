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
using Newtonsoft.Json;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class RevokeTokenClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;

        public RevokeTokenClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors

        [Fact]
        public async Task When_No_Parameters_Is_Passed_To_TokenRevoke_Edp_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/token/revoke")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            // ASSERT
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            var error = JsonConvert.DeserializeObject<ErrorResponse>(json);
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("no parameter in body request", error.ErrorDescription);
        }

        [Fact]
        public async Task When_No_Valid_Parameters_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("invalid", "invalid")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token/revoke")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            // ASSERT
            var error = JsonConvert.DeserializeObject<ErrorResponse>(json);
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter token is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Revoke_Token_And_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ex = await _clientAuthSelector.UseClientSecretPostAuth("invalid_client", "invalid_client")
                .RevokeToken("access_token", TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.True(ex.ContainsError);
            Assert.Equal("invalid_client", ex.Error.Error);
            Assert.Equal("the client doesn't exist", ex.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Token_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var ex = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .RevokeToken("access_token", TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.True(ex.ContainsError);
            Assert.Equal("invalid_token", ex.Error.Error);
            Assert.Equal("the token doesn't exist", ex.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Revoke_Token_And_Client_Is_Different_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var ex = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .RevokeToken(result.Content.AccessToken, TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.True(ex.ContainsError);
            Assert.Equal("invalid_token", ex.Error.Error);
            Assert.Equal("the token has not been issued for the given client id 'client'", ex.Error.ErrorDescription);
        }

        #endregion

        [Fact]
        public async Task When_Revoking_AccessToken_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var revoke = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .RevokeToken(result.Content.AccessToken, TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var ex = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .Introspect(result.Content.AccessToken, TokenType.AccessToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.False(revoke.ContainsError);
            Assert.True(ex.ContainsError);
        }

        [Fact]
        public async Task When_Revoking_RefreshToken_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var revoke = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .RevokeToken(result.Content.RefreshToken, TokenType.RefreshToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var ex = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .Introspect(result.Content.RefreshToken, TokenType.RefreshToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);

            // ASSERT
            Assert.False(revoke.ContainsError);
            Assert.True(ex.ContainsError);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var requestBuilder = new RequestBuilder();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }
    }
}
