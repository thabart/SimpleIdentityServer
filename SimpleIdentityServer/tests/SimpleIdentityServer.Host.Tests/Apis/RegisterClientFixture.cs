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
using Newtonsoft.Json;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    using Core.Common.DTOs.Requests;

    public class RegisterClientFixture : IClassFixture<TestOauthServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IRegistrationClient _registrationClient;
        private IClientAuthSelector _clientAuthSelector;

        public RegisterClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors

        [Fact]
        public async Task When_Empty_Json_Request_Is_Passed_To_Registration_Api_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { fake = "fake" };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_redirect_uri", error.Error);
            Assert.Equal("the parameter request_uris is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Redirect_Uris_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { redirect_uris = new[] { "invalid_redirect_uris" } };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal("invalid_redirect_uri", error.Error);
            Assert.Equal("the redirect_uri invalid_redirect_uris is not well formed", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Redirect_Uri_With_Fragment_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { redirect_uris = new[] { "http://localhost#fg=fg" } };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal("invalid_redirect_uri", error.Error);
            Assert.Equal("the redirect_uri http://localhost#fg=fg cannot contains fragment", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Logo_Uri_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { redirect_uris = new[] { "http://localhost" }, logo_uri  = "invalid_logo_uri" };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter logo_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Client_Uri_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { redirect_uris = new[] { "http://localhost" }, logo_uri = "http://google.com", client_uri = "invalid_client_uri" };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter client_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Tos_Uri_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { redirect_uris = new[] { "http://localhost" }, logo_uri = "http://google.com", client_uri = "http://google.com", tos_uri = "invalid_tos_uri" };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter tos_uri is not correct", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Invalid_Jwks_Uri_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var obj = new { redirect_uris = new[] { "http://localhost" }, logo_uri = "http://google.com", client_uri = "http://google.com", tos_uri = "http://google.com", jwks_uri = "invalid_jwks_uri" };
            var fakeJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{baseUrl}/registration"),
                Content = new StringContent(fakeJson)
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Add("Authorization", "Bearer " + grantedToken.Content.AccessToken);

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest).ConfigureAwait(false);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal("invalid_client_metadata", error.Error);
            Assert.Equal("the parameter jwks_uri is not correct", error.ErrorDescription);
        }

        #endregion

        [Fact]
        public async Task When_Registering_A_Client_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("register_client")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var client = await _registrationClient.ResolveAsync(new ClientRequest
                {
                    RedirectUris = new []
                    {
                        "https://localhost"
                    },
                    ScimProfile = true
                }, baseUrl + "/.well-known/openid-configuration", grantedToken.Content.AccessToken).ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(client);
            Assert.True(client.Content.ScimProfile);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
            _registrationClient = new RegistrationClient(new RegisterClientOperation(_httpClientFactoryStub.Object), new GetDiscoveryOperation(_httpClientFactoryStub.Object));
        }
    }
}
