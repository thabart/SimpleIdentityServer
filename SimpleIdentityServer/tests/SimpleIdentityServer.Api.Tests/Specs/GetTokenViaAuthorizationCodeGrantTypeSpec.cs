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
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.RateLimitation.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetTokenViaAuthorizationCodeGrantType")]
    public class GetTokenViaAuthorizationCodeGrantTypeSpec
    {
        private class JwsInformation
        {
            public JwsProtectedHeader Header { get; set; }

            public JwsPayload Payload { get; set; }

            public string CombinedHeaderAndPayload { get; set; }

            public string SignedPayload { get; set; }

            public string Jws { get; set; }
        }

        private ConfiguredTaskAwaitable<HttpResponseMessage> _authorizationResponseMessage;

        private ConfiguredTaskAwaitable<HttpResponseMessage> _tokenResponse;

        private Func<Task<JwsInformation>> _decryptIdTokenFromQueryStringCallback;

        private ISecurityHelper _securityHelper;

        private GlobalContext _globalContext;

        private JwsPayload _jwsPayload;
        
        private MODELS.ResourceOwner _resourceOwner;

        private string _signedPayLoad;

        private Dictionary<string, string> _tokenParameters;

        private string _clientAssertion;
        
        [BeforeScenario]
        private void Init() 
        {            
            _securityHelper = new SecurityHelper();
            _globalContext = new GlobalContext();
            _globalContext.Init();
            _globalContext.CreateServer(services => {
                var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
                {
                    Enabled = false
                };
                services.AddInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
                services.AddTransient<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
            });
        }

        #region Given

        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new MODELS.Client
            {
                ClientId = clientId,
                AllowedScopes = new List<MODELS.Scope>()
            };

            _globalContext.FakeDataSource.Clients.Add(client);
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            var resourceOwner = new MODELS.ResourceOwner
            {
                Name = userName,
                Password = _securityHelper.ComputeHash(password)
            };

            _globalContext.FakeDataSource.ResourceOwners.Add(resourceOwner);
        }

        [Given("add json web keys")]
        public void AddJsonWebKeys(Table table)
        {
            var jsonWebKeys = table.CreateSet<FakeJsonWebKey>();
            using (var provider = new RSACryptoServiceProvider())
            {
                foreach (var jsonWebKey in jsonWebKeys)
                {
                    var serializedRsa = provider.ToXmlString(true);
                    _globalContext.FakeDataSource.JsonWebKeys.Add(new MODELS.JsonWebKey
                    {
                        Alg = jsonWebKey.Alg,
                        KeyOps = new[]
                        {
                        jsonWebKey.Operation
                    },
                        Kid = jsonWebKey.Kid,
                        Kty = jsonWebKey.Kty,
                        Use = jsonWebKey.Use,
                        SerializedKey = serializedRsa
                    });
                }
            }
        }

        [Given("the scopes are defined")]
        public void GivenScope(Table table)
        {
            var scopes = table.CreateSet<FakeScope>();
            foreach (var scope in scopes)
            {
                _globalContext.FakeDataSource.Scopes.Add(scope.ToFake());
            }
        }
        
        [Given("the scopes (.*) are assigned to the client (.*)")]
        public void GivenScopesToTheClients(List<string> scopeNames, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            var scopes = _globalContext.FakeDataSource.Scopes;
            foreach (var scopeName in scopeNames)
            {
                var storedScope = scopes.SingleOrDefault(s => s.Name == scopeName);
                if (storedScope == null)
                {
                    continue;
                }

                client.AllowedScopes.Add(storedScope);
            }
        }
        
        [Given("the token endpoint authentication method (.*) is assigned to the client (.*)")]
        public void GivenTokenEndPointAuthenticationMethodIsAssigned(MODELS.TokenEndPointAuthenticationMethods tokenEndPtAuthMethod, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.TokenEndPointAuthMethod = tokenEndPtAuthMethod;
        }

        [Given("the redirection uri (.*) is assigned to the client (.*)")]
        public void GivenRedirectionUriIsAssignedTo(string redirectionUri, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.RedirectionUrls = new List<string>
            {
                redirectionUri
            };
        }

        [Given("the client secret (.*) is assigned to the client (.*)")]
        public void GivenScopesToTheClient(string clientSecret, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.ClientSecret = clientSecret;
        }

        [Given("the id_token encrypted response alg is set to (.*) for the client (.*)")]
        public void GivenEncryptedResponseAlgToTheClient(string algorithm, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenEncryptedResponseAlg = algorithm;
        }

        [Given("the id_token encrypted response enc is set to (.*) for the client (.*)")]
        public void GivenEncryptedResponseEncToTheClient(string enc, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenEncryptedResponseEnc = enc;
        }

        [Given("the id_token signature algorithm is set to (.*) for the client (.*)")]
        public void GivenIdTokenSignatureAlgorithmIsSetForTheClient(string algorithm, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenSignedResponseAlg = algorithm;
        }

        [Given("the grant-type (.*) is supported by the client (.*)")]
        public void GivenGrantTypesAreSupportedByClient(MODELS.GrantType grantType, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.GrantTypes = new List<MODELS.GrantType>
            {
                grantType
            };
        }

        [Given("the response-types (.*) are supported by the client (.*)")]
        public void GivenResponseIsSupportedByTheClient(List<string> responseTypes, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.ResponseTypes = new List<MODELS.ResponseType>();
            foreach (var responseType in responseTypes)
            {
                var resp = (MODELS.ResponseType)Enum.Parse(typeof(ResponseType), responseType);
                client.ResponseTypes.Add(resp);
            }
        }

        [Given("the consent has been given by the resource owner (.*) for the client (.*) and scopes (.*)")]
        public void GivenConsentScopes(string resourceOwnerId, string clientId, List<string> scopeNames)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            var resourceOwner = _globalContext.FakeDataSource.ResourceOwners.SingleOrDefault(r => r.Id == resourceOwnerId);
            var scopes = new List<MODELS.Scope>();
            foreach (var scopeName in scopeNames)
            {
                var storedScope = _globalContext.FakeDataSource.Scopes.SingleOrDefault(s => s.Name == scopeName);
                scopes.Add(storedScope);
            }
            var consent = new MODELS.Consent
            {
                Client = client,
                GrantedScopes = scopes,
                ResourceOwner = resourceOwner
            };

            _globalContext.FakeDataSource.Consents.Add(consent);
        }

        [Given("the consent has been given by the resource owner (.*) for the client (.*) and claims (.*)")]
        public void GivenConsentClaims(string resourceOwnerId, string clientId, List<string> claimNames)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            var resourceOwner = _globalContext.FakeDataSource.ResourceOwners.SingleOrDefault(r => r.Id == resourceOwnerId);
            var consent = new MODELS.Consent
            {
                Client = client,
                Claims = claimNames,
                ResourceOwner = resourceOwner
            };

            _globalContext.FakeDataSource.Consents.Add(consent);
        }
        
        [Given("create a resource owner")]
        public void GivenCreateAResourceOwner(Table table)
        {
            _resourceOwner = table.CreateInstance<MODELS.ResourceOwner>();
        }

        [Given("the following address is assigned to the resource owner")]
        public void GivenTheAddressIsAssignedToTheAuthenticatedResourceOwner(Table table)
        {
            var address = table.CreateInstance<MODELS.Address>();
            _resourceOwner.Address = address;
        }

        [Given("authenticate the resource owner")]
        public void GivenAuthenticateTheResourceOwner()
        {
            _globalContext.FakeDataSource.ResourceOwners.Add(_resourceOwner);
            _globalContext.AuthenticationMiddleWareOptions.IsEnabled = true;
            _globalContext.AuthenticationMiddleWareOptions.ResourceOwner = _resourceOwner;
        }
        
        [Given("requesting an authorization code")]
        public void GivenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var httpClient = _globalContext.TestServer.CreateClient();
            var url = string.Format(
                "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}",
                authorizationRequest.scope,
                authorizationRequest.response_type,
                authorizationRequest.client_id,
                authorizationRequest.redirect_uri,
                authorizationRequest.prompt,
                authorizationRequest.state,
                authorizationRequest.nonce);
            _authorizationResponseMessage = httpClient.GetAsync(url).ConfigureAwait(false);
        }

        [Given("create a request to retrieve a token")]
        public void GivenCreateRequestToRetrieveAToken(Table table)
        {
            var request = table.CreateInstance<TokenRequest>();
            _tokenParameters = new Dictionary<string, string>
            {
                {
                    "grant_type",
                    Enum.GetName(typeof (GrantTypeRequest), request.grant_type)
                },
                {
                    "redirect_uri",
                    request.redirect_uri
                },
                {
                    "client_assertion_type",
                    request.client_assertion_type
                }
            };
        }

        [Given("create the JWS payload")]
        public void GivenTheJsonWebTokenIs(Table table)
        {
            var record = table.CreateInstance<FakeJwsPayload>();
            _jwsPayload = record.ToBusiness();
        }

        [Given("assign audiences (.*) to the JWS payload")]
        public void GivenAssignAudiencesToJwsPayload(List<string> audiences)
        {
            _jwsPayload.Add(Core.Jwt.Constants.StandardClaimNames.Audiences, audiences.ToArray());
        }

        [Given("add json web keys (.*) to the client (.*)")]
        public void GivenAddJsonWeKeyToTheClient(List<string> kids, string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.FirstOrDefault(c => c.ClientId == clientId);
            client.JsonWebKeys = new List<MODELS.JsonWebKey>();
            var jsonWebKeys = _globalContext.FakeDataSource.JsonWebKeys;
            foreach (var kid in kids)
            {
                var jsonWebKey = jsonWebKeys.FirstOrDefault(j => j.Kid.Equals(kid));
                client.JsonWebKeys.Add(jsonWebKey);
            }
        }

        [Given("expiration time (.*) in seconds to the JWS payload")]
        public void GivenExpirationTimeInSecondsToJwsPayload(int expirationTimeInSeconds)
        {
            var expiration = DateTime.UtcNow.AddSeconds(expirationTimeInSeconds).ConvertToUnixTimestamp();
            _jwsPayload.Add(Core.Jwt.Constants.StandardClaimNames.ExpirationTime, expiration);
        }

        [Given("sign the jws payload with (.*) kid")]
        public void GivenSignTheJwsPayloadWithKid(string kid)
        {
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);
            var generator = _globalContext.ServiceProvider.GetService<IJwsGenerator>();
            var enumName = Enum.GetName(typeof(AllAlg), jsonWebKey.Alg);
            var alg = (JwsAlg)Enum.Parse(typeof(JwsAlg), enumName);
            _clientAssertion = generator.Generate(_jwsPayload,
                alg,
                jsonWebKey.ToBusiness());
        }

        [Given("encrypt the jws token with (.*) kid, encryption algorithm (.*) and password (.*)")]
        public void GivenEncryptTheJwsPayloadWithKid(string kid, JweEnc enc, string password)
        {
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);
            var generator = _globalContext.ServiceProvider.GetService<IJweGenerator>();
            var algEnumName = Enum.GetName(typeof(AllAlg), jsonWebKey.Alg);
            var alg = (JweAlg)Enum.Parse(typeof(JweAlg), algEnumName);
            _clientAssertion = generator.GenerateJweByUsingSymmetricPassword(
                _clientAssertion,
                alg,
                enc,
                jsonWebKey.ToBusiness(),
                password);
        }

        [Given("set the client assertion value")]
        public void GivenSetClientAssertion()
        {
            _tokenParameters.Add("client_assertion", _clientAssertion);
        }

        [Given("set the client id (.*) into the request")]
        public void GivenSetTheClientIdIntoTheRequest(string clientId)
        {
            _tokenParameters.Add("client_id", clientId);
        }

        #endregion

        #region When requests

        [When("retrieve token via client assertion authentication")]
        public async Task WhenRetrieveTokenViaClientAssertionAuthentication()
        {
            var authorizationResponse = await _authorizationResponseMessage;
            var query = HttpUtility.ParseQueryString(authorizationResponse.Headers.Location.Query.TrimStart('#'));
            var authorizationCode = query["code"];
            _tokenParameters.Add("code", authorizationCode);
            var httpClient = _globalContext.TestServer.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            _tokenResponse = httpClient.PostAsync("/token", new FormUrlEncodedContent(_tokenParameters)).ConfigureAwait(false);
            // _grantedToken = await response.Content.ReadAsAsync<DOMAINS.GrantedToken>().ConfigureAwait(false);
        }

        [When("requesting a token with basic client authentication for the client id (.*) and client secret (.*)")]
        public async Task WhenRequestingATokenWithTheAuthorizationCodeFlow(
            string clientId, 
            string clientSecret, 
            Table table)
        {
            var authorizationResponse = await _authorizationResponseMessage;
            var request = table.CreateInstance<TokenRequest>();
            var query = HttpUtility.ParseQueryString(authorizationResponse.Headers.Location.Query);
            var authorizationCode = query["code"];
            request.code = authorizationCode;

            var dic = new Dictionary<string, string>
            {
                {
                    "grant_type",
                    Enum.GetName(typeof (GrantTypeRequest), request.grant_type)
                },
                {
                    "client_id",
                    request.client_id
                },
                {
                    "code",
                    authorizationCode
                },
                {
                    "redirect_uri",
                    request.redirect_uri
                }
            };

            var httpClient = _globalContext.TestServer.CreateClient();
            var credentials = clientId + ":" + clientSecret;
            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Basic {0}", credentials.Base64Encode()));

            _tokenResponse = httpClient.PostAsync("/token", new FormUrlEncodedContent(dic)).ConfigureAwait(false);
            // _grantedToken = await response.Content.ReadAsAsync<DOMAINS.GrantedToken>().ConfigureAwait(false);
        }

        [When("requesting a token by using a client_secret_post authentication mechanism")]
        public async Task WhenRequestingATokenByUsingClientSecretPostAuthMech(
            Table table)
        {
            var authorizationResponse = await _authorizationResponseMessage;
            var request = table.CreateInstance<TokenRequest>();
            var query = HttpUtility.ParseQueryString(authorizationResponse.Headers.Location.Query);
            var authorizationCode = query["code"];
            request.code = authorizationCode;

            var dic = new Dictionary<string, string>
            {
                {
                    "grant_type",
                    Enum.GetName(typeof (GrantTypeRequest), request.grant_type)
                },
                {
                    "client_id",
                    request.client_id
                },
                {
                    "client_secret",
                    request.client_secret
                },
                {
                    "code",
                    authorizationCode
                },
                {
                    "redirect_uri",
                    request.redirect_uri
                }
            };

            var httpClient = _globalContext.TestServer.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            _tokenResponse = httpClient.PostAsync("/token", new FormUrlEncodedContent(dic)).ConfigureAwait(false);
            // _grantedToken = await response.Content.ReadAsAsync<DOMAINS.GrantedToken>().ConfigureAwait(false);
        }

        #endregion

        #region Then requests

        [Then("the following token is returned")]
        public async Task ThenTheCallbackContainsTheQueryName(Table table)
        {
            var tokenResponse = await _tokenResponse;
            var grantedToken = await tokenResponse.Content.ReadAsAsync<DOMAINS.GrantedToken>().ConfigureAwait(false);
            var record = table.CreateInstance<DOMAINS.GrantedToken>();

            Assert.True(record.TokenType.Equals(grantedToken.TokenType));
        }

        [Then("decrypt the id_token parameter from the response")]
        public void ThenDecryptTheIdTokenFromTheQueryString()
        {
            _decryptIdTokenFromQueryStringCallback = async () =>
            {
                var tokenResponse = await _tokenResponse;
                var grantedToken = await tokenResponse.Content.ReadAsAsync<DOMAINS.GrantedToken>().ConfigureAwait(false);

                Assert.NotNull(grantedToken.IdToken);

                var parts = grantedToken.IdToken.Split('.');

                Assert.True(parts.Length >= 3);

                var secondPart = parts[1].Base64Decode();

                var javascriptSerializer = new JavaScriptSerializer();
                return new JwsInformation
                {
                    Header = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode()),
                    Payload = javascriptSerializer.Deserialize<JwsPayload>(secondPart),
                    CombinedHeaderAndPayload = parts[0] + "." + parts[1],
                    SignedPayload = parts[2]
                };
            };

        }

        [Then("the signature of the JWS payload is valid")]
        public async Task ThenTheSignatureIsCorrect()
        {
            var jwsInformation = await _decryptIdTokenFromQueryStringCallback();
            using (var provider = new RSACryptoServiceProvider())
            {
                var serializedRsa = _globalContext.FakeDataSource.JsonWebKeys.First().SerializedKey;
                provider.FromXmlString(serializedRsa);
                var signature = _signedPayLoad.Base64DecodeBytes();
                var payLoad = ASCIIEncoding.ASCII.GetBytes(jwsInformation.CombinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.True(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public async Task ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var jwsInformation = await _decryptIdTokenFromQueryStringCallback();
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.True(record.Alg.Equals(jwsInformation.Header.Alg));
        }

        [Then("the parameter nonce with value (.*) is returned by the JWS payload")]
        public async Task ThenNonceIsReturnedInJwsPayLoad(string nonce)
        {
            var jwsInformation = await _decryptIdTokenFromQueryStringCallback();
            Assert.True(jwsInformation.Payload.Nonce.Equals(nonce));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public async Task ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var jwsInformation = await _decryptIdTokenFromQueryStringCallback();
            var claimValue = jwsInformation.Payload.GetClaimValue(claimName);

            Assert.NotNull(claimValue);
            Assert.True(claimValue.Equals(val));
        }

        #endregion

        [AfterScenario]
        public void After()
        {            
            _globalContext.TestServer.Dispose();
        }

        private MODELS.Client GetClient(string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }
    }
}
