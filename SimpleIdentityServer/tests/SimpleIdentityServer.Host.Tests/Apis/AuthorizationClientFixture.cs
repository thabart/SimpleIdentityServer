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

using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Builders;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Host.Tests.MiddleWares;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class AuthorizationClientFixture : IClassFixture<TestOauthServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IAuthorizationClient _authorizationClient;
        private IClientAuthSelector _clientAuthSelector;
        private IJwsGenerator _jwsGenerator;
        private IJweGenerator _jweGenerator;

        public AuthorizationClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors

        [Fact]
        public async Task When_Scope_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization" ));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter scope is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_ClientId_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter client_id is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_RedirectUri_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&client_id=client"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter redirect_uri is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_ResponseType_IsNot_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&client_id=client&redirect_uri=redirect_uri"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter response_type is missing", error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unsupported_ResponseType_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=client&redirect_uri=redirect_uri&response_type=invalid"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("at least one response_type parameter is not supported", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_UnsupportedPrompt_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=client&redirect_uri=redirect_uri&response_type=token&prompt=invalid"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("at least one prompt parameter is not supported", error.ErrorDescription);
            Assert.Equal("state", error.State);

        }

        [Fact]
        public async Task When_Not_Correct_Redirect_Uri_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=client&redirect_uri=redirect_uri&response_type=token&prompt=none"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("Based on the RFC-3986 the redirection-uri is not well formed", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Not_Correct_ClientId_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();


            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=bad_client&redirect_uri=http://localhost:5000&response_type=token&prompt=none"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the client id parameter bad_client doesn't exist or is not valid", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Not_Support_Redirect_Uri_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();


            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=pkce_client&redirect_uri=http://localhost:5000&response_type=token&prompt=none"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the redirect url http://localhost:5000 doesn't exist or is not valid", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_ClientRequiresPkce_And_No_CodeChallenge_Is_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=pkce_client&redirect_uri=http://localhost:5000/callback&response_type=token&prompt=none"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the client pkce_client requires PKCE", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Nonce_Parameter_Is_Not_Passed_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the parameter nonce is missing", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Pass_Invalid_Scope_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=scope&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none&nonce=nonce"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_scope", error.Error);
            Assert.Equal("the scopes scope are not allowed or invalid", error.ErrorDescription);
            Assert.Equal("state", error.State);
        }

        [Fact]
        public async Task When_Use_Hybrid_And_Dont_Pass_Not_Supported_ResponseTypes_To_Authorization_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var httpResult = await _server.Client.GetAsync(new Uri(baseUrl + "/authorization?scope=openid api1&state=state&client_id=incomplete_authcode_client&redirect_uri=http://localhost:5000/callback&response_type=id_token code token&prompt=none&nonce=nonce"));
            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<ErrorResponseWithState>(json);

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("invalid_request", error.Error);
            Assert.Equal("the client 'incomplete_authcode_client' doesn't support the response type: 'id_token,code,token'", error.ErrorDescription);
            Assert.Equal("state", error.State);
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
            Assert.True(result.ContainsError);
            Assert.True(result.Error.Error == "invalid_request");
        }

        #region Prompts

        [Fact]
        public async Task When_User_Is_Not_Authenticated_And_Pass_None_Prompt_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            UserStore.Instance().IsInactive = true;
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.None
                });
            UserStore.Instance().IsInactive = false;

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("login_required", result.Error.Error);
            Assert.Equal("the user needs to be authenticated", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_User_Is_Authenticated__And_Pass_Prompt_And_No_Consent_Has_Been_Given_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            UserStore.Instance().Subject = "user";
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.None
                });
            UserStore.Instance().Subject = "administrator";

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("interaction_required", result.Error.Error);
            Assert.Equal("the user needs to give his consent", result.Error.ErrorDescription);
        }

        #endregion

        #region id_token_hint

        [Fact]
        public async Task When_Pass_Invalid_IdTokenHint_To_Authorization_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    IdTokenHint = "token",
                    Prompt = "none"
                });

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the id_token_hint parameter is not a valid token", result.Error.ErrorDescription);
        }
        
        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Audience_Is_Not_Correct_Then_Error_Is_Returned()
        {
            // GENERATE JWS
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    "sub", "administrator"
                }
            };
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    IdTokenHint = jws,
                    Prompt = "none"
                });

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the identity token doesnt contain simple identity server in the audience", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Subject_Doesnt_Match_Then_Error_Is_Returned()
        {
            // GENERATE JWS
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    "sub", "adm"
                }
            };
            payload.Add("aud", new[] { "http://localhost:5000" });
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    IdTokenHint = jws,
                    Prompt = "none"
                });

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the current authenticated user doesn't match with the identity token", result.Error.ErrorDescription);
        }
        
        #endregion

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Requesting_AuthorizationCode_Then_Code_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.None
                });
            Uri location = result.Location;
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(location.Query);
            var token = await _clientAuthSelector.UseClientSecretPostAuth("authcode_client", "authcode_client")
                .UseAuthorizationCode(queries["code"], "http://localhost:5000/callback")
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotNull(result.Location);
            Assert.NotNull(token);
            Assert.NotEmpty(token.Content.AccessToken);
            Assert.True(queries["state"] == "state");
        }

        #region max_age

        [Fact]
        public async Task When_Pass_MaxAge_And_User_Session_Is_Inactive_Then_Redirect_To_Authenticate_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            UserStore.Instance().AuthenticationOffset = DateTimeOffset.UtcNow.AddDays(-2);
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.None,
                    MaxAge = 300
                });
            Uri location = result.Location;
            UserStore.Instance().AuthenticationOffset = null;

            // ASSERT
            Assert.Equal("/pwd/Authenticate/OpenId", location.LocalPath);
        }

        #endregion

        #region prompts

        [Fact]
        public async Task When_Pass_Login_Prompt_Then_Redirect_To_Authenticate_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.Login
                });

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("/pwd/Authenticate/OpenId", result.Location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Consent_Prompt_And_User_Is_Not_Authenticated_Then_Redirect_To_Authenticate_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            UserStore.Instance().IsInactive = true;
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.Consent
                });
            UserStore.Instance().IsInactive = false;

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("/pwd/Authenticate/OpenId", result.Location.LocalPath);
        }

        [Fact]
        public async Task When_Pass_Consent_Prompt_And_User_Is_Authenticated_Then_Redirect_To_Authenticate_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);


            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.Consent
                });

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("/Consent", result.Location.LocalPath);
        }

        #endregion

        #region id_token_hint

        [Fact]
        public async Task When_Pass_IdTokenHint_And_The_Subject_Matches_The_Authenticated_User_Then_Token_Is_Returned()
        {
            // GENERATE JWS
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var payload = new JwsPayload
            {
                {
                    "sub", "administrator"
                }
            };
            payload.Add("aud", new[] { "http://localhost:5000" });
            var jws = _jwsGenerator.Generate(payload, JwsAlg.RS256, _server.SharedCtx.SignatureKey);
            var jwe = _jweGenerator.GenerateJwe(jws, JweAlg.RSA1_5, JweEnc.A128CBC_HS256, _server.SharedCtx.EncryptionKey);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.Code }, "authcode_client", "http://localhost:5000/callback", "state")
                {
                    IdTokenHint = jwe,
                    Prompt = "none"
                });

            // ASSERT
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        #endregion

        [Fact]
        public async Task When_Requesting_Token_And_CodeVerifier_Is_Passed_Then_Token_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var builder = new PkceBuilder();
            var pkce = builder.Build(CodeChallengeMethods.S256);

            // ACT
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration", new AuthorizationRequest(
                new[] { "openid", "api1" },
                new[] { ResponseTypes.Code },
                "pkce_client", "http://localhost:5000/callback",
                "state")
            {
                CodeChallenge = pkce.CodeChallenge,
                CodeChallengeMethod = CodeChallengeMethods.S256,
                Prompt = PromptNames.None
            });
            Uri location = result.Location;
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(location.Query);
            var token = await _clientAuthSelector.UseClientSecretPostAuth("pkce_client", "pkce_client")
                .UseAuthorizationCode(queries["code"], "http://localhost:5000/callback", pkce.CodeVerifier)
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration");

            // ASSERT
            Assert.NotNull(token);
            Assert.NotEmpty(token.Content.AccessToken);
        }

        [Fact]
        public async Task When_Requesting_IdTokenAndAccessToken_Then_Tokens_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.IdToken, ResponseTypes.Token }, "implicit_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.None,
                    Nonce = "nonce"
                });
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(result.Location.Fragment.TrimStart('#'));

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotNull(result.Location);
            Assert.True(queries.ContainsKey("id_token"));
            Assert.True(queries.ContainsKey("access_token"));
            Assert.True(queries.ContainsKey("state"));
            Assert.True(queries["state"] == "state");
        }

        [Fact]
        public async Task When_RequestingIdTokenAndAuthorizationCodeAndAccessToken_Then_Tokens_Are_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            // NOTE : The consent has already been given in the database.
            var result = await _authorizationClient.ResolveAsync(baseUrl + "/.well-known/openid-configuration",
                new AuthorizationRequest(new[] { "openid", "api1" }, new[] { ResponseTypes.IdToken, ResponseTypes.Token, ResponseTypes.Code }, "hybrid_client", "http://localhost:5000/callback", "state")
                {
                    Prompt = PromptNames.None,
                    Nonce = "nonce"
                });
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(result.Location.Fragment.TrimStart('#'));

            // ASSERTS
            Assert.NotNull(result);
            Assert.NotNull(result.Location);
            Assert.True(queries.ContainsKey("id_token"));
            Assert.True(queries.ContainsKey("access_token"));
            Assert.True(queries.ContainsKey("code"));
            Assert.True(queries.ContainsKey("state"));
            Assert.True(queries["state"] == "state");
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
            var getAuthorizationOperation = new GetAuthorizationOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _authorizationClient = new AuthorizationClient(getAuthorizationOperation, getDiscoveryOperation);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }
    }
}
