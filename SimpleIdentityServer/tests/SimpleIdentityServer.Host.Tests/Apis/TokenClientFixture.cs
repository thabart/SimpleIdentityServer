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

using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using SimpleIdentityServer.Authenticate.SMS.Client;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Store;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class TokenClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private Mock<Authenticate.SMS.Client.Factories.IHttpClientFactory> _smsHttpClientFactoryStub;
        private IClientAuthSelector _clientAuthSelector;
        private IUserInfoClient _userInfoClient;
        private ISidSmsAuthenticateClient _sidSmsAuthenticateClient;
        private IJwsGenerator _jwsGenerator;
        private IJweGenerator _jweGenerator;

        public TokenClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors password grant_type

        [Fact]
        public async Task When_GrantType_Is_Not_Specified_To_Token_Endpoint_Then_Json_Is_Returned()
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
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter grant_type is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_No_Username_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter username is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_No_Password_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter password is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_No_Scope_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter scope is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_Invalid_ClientId_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("client_id", "invalid_client_id")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Error);
            Assert.Equal("the client doesn't exist", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_Authenticate_Client_With_Not_Accepted_Auth_Method_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("client_id", "basic_client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Error);
            Assert.Equal("the client cannot be authenticated with secret basic", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_ResourceOwner_Credentials_Are_Not_Valid_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "invalid_password"),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_grant", error.Error);
            Assert.Equal("resource owner credentials are not valid", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_Password_GrantType_And_Scopes_Are_Not_Valid_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "administrator"),
                new KeyValuePair<string, string>("password", "password"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("scope", "invalid"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_scope", error.Error);
            Assert.Equal("the scopes invalid are not allowed or invalid", error.ErrorDescription);
        }

        #endregion

        #region Errors client credentials grant_type

        [Fact]
        public async Task When_Use_ClientCredentials_Grant_Type_And_No_Scope_Is_Passwed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter scope is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_ClientCredentials_And_Client_Doesnt_Support_It_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "invalid_scope"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Error);
            Assert.Equal("the client client doesn't support the grant type client_credentials", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_ClientCredentials_And_Client_Doesnt_Have_Token_ResponseType_It_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "invalid_scope"),
                new KeyValuePair<string, string>("client_id", "clientWithWrongResponseType"),
                new KeyValuePair<string, string>("client_secret", "clientWithWrongResponseType")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Error);
            Assert.Equal("the client 'clientWithWrongResponseType' doesn't support the response type: 'token'", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_ClientCredentials_And_Scope_Is_Not_Supported_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "invalid"),
                new KeyValuePair<string, string>("client_id", "clientCredentials"),
                new KeyValuePair<string, string>("client_secret", "clientCredentials")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_scope", error.Error);
            Assert.Equal("the scopes invalid are not allowed or invalid", error.ErrorDescription);
        }

        #endregion

        #region Errors refresh token

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_No_RefreshToken_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter refresh_token is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_Invalid_ClientId_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", "invalid_refresh_token"),
                new KeyValuePair<string, string>("client_id", "invalid_client_id")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_client", error.Error);
            Assert.Equal("the client doesn't exist", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_RefreshToken_Doesnt_Exist_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", "invalid_refresh_token"),
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_grant", error.Error);
            Assert.Equal("the refresh token is not valid", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_RefreshToken_Grant_Type_And_Another_Client_Tries_ToRefresh_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("openid")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var refreshToken = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UseRefreshToken(result.Content.RefreshToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, refreshToken.Status);
            Assert.Equal("invalid_grant", refreshToken.Error.Error);
            Assert.Equal("the refresh token can be used only by the same issuer", refreshToken.Error.ErrorDescription);
        }

        #endregion

        #region Errors authorization code
        
        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_No_Code_Is_Passed_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter code is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_RedirectUri_Is_Invalid_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("Based on the RFC-3986 the redirection-uri is not well formed", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Use_AuthCode_Grant_Type_And_Code_Doesnt_Exist_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var request = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "code"),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5000/callback"),
                new KeyValuePair<string, string>("client_id", "authcode_client"),
                new KeyValuePair<string, string>("client_secret", "authcode_client")
            };
            var body = new FormUrlEncodedContent(request);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = body,
                RequestUri = new Uri($"{baseUrl}/token")
            };

            // ACT
            var httpResult = await _server.Client.SendAsync(httpRequest);
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERTS
            Assert.Equal(HttpStatusCode.BadRequest, httpResult.StatusCode);
            Assert.Equal("invalid_grant", error.Error);
            Assert.Equal("the authorization code is not correct", error.ErrorDescription);
        }

        // TH : CONTINUE TO WRITE UTS

        #endregion

        #region GrantTypes

        [Fact]
        public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("openid")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            // var claims = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.AccessToken);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_Password_Grant_Type_Then_Access_Token_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            // var claims = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.AccessToken);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_Password_Grant_Type_With_SMS_Then_Access_Token_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _smsHttpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            ConfirmationCode confirmationCode = new ConfirmationCode();
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult((ConfirmationCode)null);
            });
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>())).Callback<ConfirmationCode>(r =>
            {
                confirmationCode = r;
            }).Returns(() =>
            {
                return Task.FromResult(true);
            });
            await _sidSmsAuthenticateClient.Send(baseUrl, new Authenticate.SMS.Common.Requests.ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            });
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>())).Returns(Task.FromResult(confirmationCode));
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("phone", confirmationCode.Value, new List<string> { "sms" }, new List<string> { "scim" })
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_Client_Certificate_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var certificate = new X509Certificate2("testCert.pfx");

            // ACT
            var result = await _clientAuthSelector.UseClientCertificate("certificate_client", certificate)
                .UsePassword("administrator", "password", "openid")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_RefreshToken_GrantType_Then_New_One_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UsePassword("administrator", "password", "scim")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");
            var refreshToken = await _clientAuthSelector.UseClientSecretPostAuth("client", "client")
                .UseRefreshToken(result.Content.RefreshToken)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        #endregion

        #region Client authentications

        [Fact]
        public async Task When_Using_ClientSecretPostAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var token = await _clientAuthSelector.UseClientSecretBasicAuth("basic_client", "basic_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(token);
            Assert.False(token.ContainsError);
            Assert.NotEmpty(token.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_BaseAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            var firstToken = await _clientAuthSelector.UseClientSecretBasicAuth("basic_client", "basic_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(firstToken);
            Assert.False(firstToken.ContainsError);
            Assert.NotEmpty(firstToken.Content.AccessToken);
        }
        
        [Fact]
        public async Task When_Using_ClientSecretJwtAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "jwt_client"
                },
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "jwt_client"
                },
                {
                    StandardClaimNames.Audiences, new []
                    {
                        "http://localhost:5000"
                    }
                },
                {
                    StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddHours(1).ConvertToUnixTimestamp()
                }
            };
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);
            var jwe = _jweGenerator.GenerateJweByUsingSymmetricPassword(jws, JweAlg.RSA1_5, JweEnc.A128CBC_HS256, _server.SharedCtx.EncryptionKey, "jwt_client");

            // ACT
            var token = await _clientAuthSelector.UseClientSecretJwtAuth(jwe, "jwt_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");


            // ASSERTS
            Assert.NotNull(token);
            Assert.False(token.ContainsError);
        }

        [Fact]
        public async Task When_Using_PrivateKeyJwtAuthentication_Then_AccessToken_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "private_key_client"
                },
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "private_key_client"
                },
                {
                    StandardClaimNames.Audiences, new []
                    {
                        "http://localhost:5000"
                    }
                },
                {
                    StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddHours(1).ConvertToUnixTimestamp()
                }
            };
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var token = await _clientAuthSelector.UseClientPrivateKeyAuth(jws, "private_key_client")
                .UseClientCredentials("api1")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(token);
            Assert.False(token.ContainsError);
            Assert.NotEmpty(token.Content.AccessToken);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            var services = new ServiceCollection();
            services.AddSimpleIdentityServerJwt();
            var provider = services.BuildServiceProvider();
            _jwsGenerator = (IJwsGenerator)provider.GetService(typeof(IJwsGenerator));
            _jweGenerator = (IJweGenerator)provider.GetService(typeof(IJweGenerator));
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _smsHttpClientFactoryStub = new Mock<Authenticate.SMS.Client.Factories.IHttpClientFactory>();
            var requestBuilder = new RequestBuilder();
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            var sendSmsOperation = new SendSmsOperation(_smsHttpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation), 
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
            var getUserInfoOperation = new GetUserInfoOperation(_httpClientFactoryStub.Object);
            _sidSmsAuthenticateClient = new SidSmsAuthenticateClient(sendSmsOperation);
            _userInfoClient = new UserInfoClient(getUserInfoOperation, getDiscoveryOperation);
        }
    }
}
