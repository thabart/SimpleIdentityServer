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
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.RateLimitation.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetIdTokenViaImplicitWorkflow")]
    public sealed class GetIdTokenViaImplicitWorkflowSpec
    {
        private class JwsInformation
        {
            public JwsProtectedHeader Header { get; set; }

            public JwsPayload Payload { get; set; }

            public string CombinedHeaderAndPayload { get; set; }

            public string SignedPayload { get; set; }

            public string Jws { get; set; }
        }

        private GlobalContext _globalContext;
        
        private ISecurityHelper _securityHelper;

        private FakeHttpClientFactory _fakeHttpClientFactory;

        private ConfiguredTaskAwaitable<HttpResponseMessage> _httpResponseMessage;

        private Func<Task<JwsInformation>> _getJwsInformationCallback;

        private MODELS.ResourceOwner _resourceOwner;

        private string _combinedHeaderAndPayload;

        private AuthorizationRequest _authorizationRequest;

        private string _request;

        [BeforeScenario]
        private void Init() 
        {            
            _securityHelper = new SecurityHelper();
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };

            _fakeHttpClientFactory = new FakeHttpClientFactory();
            
            _globalContext = new GlobalContext();
            _globalContext.Init();
            _globalContext.CreateServer(services =>
            {                
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

        [Given("create an authorization request")]
        public void GivenCreateAnAuthorizationRequest(Table table)
        {
            _authorizationRequest = table.CreateInstance<AuthorizationRequest>();
        }

        [Given("sign the authorization request with (.*) kid and algorithm (.*)")]
        public void GivenSignTheAuthorizationRequestWithKid(string kid, JwsAlg jwsAlg)
        {
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            var jwsPayload = _authorizationRequest.ToJwsPayload();
            Assert.NotNull(jsonWebKey);
            var jwsGenerator = _globalContext.ServiceProvider.GetService<IJwsGenerator>();
            _request = jwsGenerator.Generate(jwsPayload, jwsAlg, jsonWebKey.ToBusiness());

           _globalContext.FakeDataSource.Clients.First().JsonWebKeys = _globalContext.FakeDataSource.JsonWebKeys;
        }

        [Given("encrypt the authorization request with (.*) kid, JweAlg: (.*) and JweEnc: (.*)")]
        public void GivenSignTheAuthorizationRequestWithKid(string kid, JweAlg jweAlg, JweEnc jweEnc)
        {
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);
            var jweGenerator = _globalContext.ServiceProvider.GetService<IJweGenerator>();
            _request = jweGenerator.GenerateJwe(_request, jweAlg, jweEnc, jsonWebKey.ToBusiness());
        }

        [Given("set the request parameter with signed AND/OR encrypted authorization request")]
        public void GivenSetTheRequestParameterWithEncryptedAndOrSignedAuthorizationRequest()
        {
            _authorizationRequest.request = _request;
        }

        #endregion

        #region When

        [When("requesting an authorization")]
        public void WhenRequestingAnAuthorizationCode()
        {            
            var httpClient = _globalContext.TestServer.CreateClient();
            _fakeHttpClientFactory.HttpClient = httpClient;
            var url = string.Format(
                "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}&claims={7}&request={8}&request_uri={9}",
                _authorizationRequest.scope,
                _authorizationRequest.response_type,
                _authorizationRequest.client_id,
                _authorizationRequest.redirect_uri,
                _authorizationRequest.prompt,
                _authorizationRequest.state,
                _authorizationRequest.nonce,
                _authorizationRequest.claims,
                _authorizationRequest.request,
                _authorizationRequest.request_uri);
            _httpResponseMessage = httpClient.GetAsync(url).ConfigureAwait(false);
        }

        #endregion

        #region Then

        [Then("the http status code is (.*)")]
        public async Task ThenHttpStatusCodeIsCorrect(HttpStatusCode code)
        {
            var httpResponseMessage = await _httpResponseMessage;
            Assert.True(code.Equals(httpResponseMessage.StatusCode));
        }

        [Then("the error code is (.*)")]
        public async Task ThenTheErrorCodeIs(string errorCode)
        {
            var httpResponseMessage = await _httpResponseMessage;
            var errorResponse = await httpResponseMessage.Content.ReadAsAsync<ErrorResponse>().ConfigureAwait(false);
            Assert.NotNull(errorResponse);
            Assert.True(errorResponse.error.Equals(errorCode));
        }

        [Then("redirect to (.*) controller")]
        public async Task ThenRedirectToController(string controller)
        {
            var httpResponseMessage = await _httpResponseMessage;
            var location = httpResponseMessage.Headers.Location;
            Assert.True(location.AbsolutePath.Equals(controller));
        }

        [Then("decrypt the id_token parameter from the fragment")]
        public void ThenDecryptTheIdTokenFromTheFragment()
        {
            _getJwsInformationCallback = async() =>
            {
                var httpResponseMessage = await _httpResponseMessage;
                var location = httpResponseMessage.Headers.Location;
                var query = HttpUtility.ParseQueryString(location.Fragment.TrimStart('#'));
                var idToken = query["id_token"];

                Assert.NotNull(idToken);

                var parts = idToken.Split('.');

                Assert.True(parts.Count() >= 3);

                var secondPart = parts[1].Base64Decode();

                var javascriptSerializer = new JavaScriptSerializer();
                var jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
                return new JwsInformation
                {
                    Header = jwsProtectedHeader,
                    Payload = javascriptSerializer.Deserialize<JwsPayload>(secondPart),
                    CombinedHeaderAndPayload = parts[0] + "." + parts[1],
                    SignedPayload = parts[2],
                    Jws = idToken
                };
            };
        }
        
        [Then("decrypt the jwe parameter from the fragment with the following kid (.*)")]
        public void ThenDecryptTheJweParameterFromTheFragment(string kid)
        {
            _getJwsInformationCallback = async () =>
            {
                var httpResponseMessage = await _httpResponseMessage;
                var location = httpResponseMessage.Headers.Location;
                var query = HttpUtility.ParseQueryString(location.Fragment.TrimStart('#'));
                var idToken = query["id_token"];

                Assert.NotNull(idToken);

                var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
                Assert.NotNull(jsonWebKey);

                var jweParser = _globalContext.ServiceProvider.GetService<IJweParser>();
                var result = jweParser.Parse(idToken, jsonWebKey.ToBusiness());
                var parts = result.Split('.');

                Assert.True(parts.Count() >= 3);

                var secondPart = parts[1].Base64Decode();

                var javascriptSerializer = new JavaScriptSerializer();
                var jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
                return new JwsInformation
                {
                    Header = jwsProtectedHeader,
                    Payload = javascriptSerializer.Deserialize<JwsPayload>(secondPart),
                    CombinedHeaderAndPayload = parts[0] + "." + parts[1],
                    SignedPayload = parts[2],
                    Jws = result
                };
            };
        }

        [Then("check the signature is correct with the kid (.*)")]
        public async Task ThenCheckSignatureIsCorrectWithKid(string kid)
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);

            var jwsParser = _globalContext.ServiceProvider.GetService<IJwsParser>();
            var result = jwsParser.ValidateSignature(jwsInformation.Jws, jsonWebKey.ToBusiness());

            Assert.NotNull(result);
        }

        [Then("the signature of the JWS payload is valid")]
        public async Task ThenTheSignatureIsCorrect()
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            using (var provider = new RSACryptoServiceProvider())
            {
                var serializedRsa = _globalContext.FakeDataSource.JsonWebKeys.First().SerializedKey;
                provider.FromXmlString(serializedRsa);
                var signature = jwsInformation.SignedPayload.Base64DecodeBytes();
                var payLoad = ASCIIEncoding.ASCII.GetBytes(_combinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.True(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public async Task ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.True(record.Alg.Equals(jwsInformation.Header.Alg));
        }

        [Then("the audience parameter with value (.*) is returned by the JWS payload")]
        public async Task ThenAudienceIsReturnedInJwsPayLoad(string audience)
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            Assert.True(jwsInformation.Payload.Audiences.Contains(audience));
        }

        [Then("the parameter nonce with value (.*) is returned by the JWS payload")]
        public async Task ThenNonceIsReturnedInJwsPayLoad(string nonce)
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            Assert.True(jwsInformation.Payload.Nonce.Equals(nonce));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public async Task ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            var claimValue = jwsInformation.Payload.GetClaimValue(claimName);

            Assert.NotNull(claimValue);
            Assert.True(claimValue.Equals(val));
        }

        [Then("the JWS payload contains (.*) claims")]
        public async Task ThenTheJwsPayloadContainsNumberOfClaims(int numberOfClaims)
        {
            var jwsInformation = await _getJwsInformationCallback().ConfigureAwait(false);
            Assert.True(jwsInformation.Payload.Count.Equals(numberOfClaims));
        }

        [Then("the callback contains the following query name (.*)")]
        public async Task ThenTheCallbackContainsTheQueryName(string queryName)
        {
            var httpResponseMessage = await _httpResponseMessage;
            var location = httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Fragment.TrimStart('#'));
            var queryValue = query[queryName];
            Assert.NotEmpty(queryValue);
        }

        #endregion

        private MODELS.Client GetClient(string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }
        
        [AfterScenario]
        public void After() 
        {
            Console.WriteLine("dispose");
            _globalContext.TestServer.Dispose();
        }
    }
}
