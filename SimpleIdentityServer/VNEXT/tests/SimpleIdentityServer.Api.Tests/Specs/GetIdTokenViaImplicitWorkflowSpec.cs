using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.RateLimitation.Configuration;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

using System.Web;
using System.Web.Script.Serialization;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using SimpleIdentityServer.Core.Helpers;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetIdTokenViaImplicitWorkflow")]
    public sealed class GetIdTokenViaImplicitWorkflowSpec
    {
        private GlobalContext _globalContext;
        
        private ISecurityHelper _securityHelper;

        private FakeHttpClientFactory _fakeHttpClientFactory;

        private HttpResponseMessage _httpResponseMessage;

        private MODELS.ResourceOwner _resourceOwner;

        private JwsProtectedHeader _jwsProtectedHeader;

        private string _jws;

        private JwsPayload _jwsPayLoad;

        private string _signedPayLoad;

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
            Console.WriteLine("requesting ..." + url);
            _httpResponseMessage = httpClient.GetAsync(url).Result;
        }
        
        [Then("the http status code is (.*)")]
        public void ThenHttpStatusCodeIsCorrect(HttpStatusCode code)
        {
            Assert.True(code.Equals(_httpResponseMessage.StatusCode));
        }

        [Then("the error code is (.*)")]
        public void ThenTheErrorCodeIs(string errorCode)
        {
            var errorResponse = _httpResponseMessage.Content.ReadAsAsync<ErrorResponse>().Result;
            Assert.NotNull(errorResponse);
            Assert.True(errorResponse.error.Equals(errorCode));
        }

        [Then("redirect to (.*) controller")]
        public void ThenRedirectToController(string controller)
        {
            var location = _httpResponseMessage.Headers.Location;
            Assert.True(location.AbsolutePath.Equals(controller));
        }

        [Then("decrypt the id_token parameter from the fragment")]
        public void ThenDecryptTheIdTokenFromTheQueryString()
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Fragment.TrimStart('#'));
            var idToken = query["id_token"];

            Assert.NotNull(idToken);

            var parts = idToken.Split('.');

            Assert.True(parts.Count() >= 3);
            _jws = idToken;

            var secondPart = parts[1].Base64Decode();

            var javascriptSerializer = new JavaScriptSerializer();
            _jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
            _jwsPayLoad = javascriptSerializer.Deserialize<JwsPayload>(secondPart);
            _combinedHeaderAndPayload = parts[0] + "." + parts[1];
            _signedPayLoad = parts[2];
        }

        [Then("decrypt the jwe parameter from the fragment with the following kid (.*)")]
        public void ThenDecryptTheJweParameterFromTheQueryString(string kid)
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Fragment.TrimStart('#'));
            var idToken = query["id_token"];

            Assert.NotNull(idToken);

            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);

            var jweParser = _globalContext.ServiceProvider.GetService<IJweParser>();
            var result = jweParser.Parse(idToken, jsonWebKey.ToBusiness());
            _jws = result;
            var parts = result.Split('.');

            Assert.True(parts.Count() >= 3);

            var secondPart = parts[1].Base64Decode();

            var javascriptSerializer = new JavaScriptSerializer();
            _jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
            _jwsPayLoad = javascriptSerializer.Deserialize<JwsPayload>(secondPart);
            _combinedHeaderAndPayload = parts[0] + "." + parts[1];
            _signedPayLoad = parts[2];
        }

        [Then("check the signature is correct with the kid (.*)")]
        public void ThenCheckSignatureIsCorrectWithKid(string kid)
        {
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);

            var jwsParser = _globalContext.ServiceProvider.GetService<IJwsParser>();
            var result = jwsParser.ValidateSignature(_jws, jsonWebKey.ToBusiness());

            Assert.NotNull(result);
        }

        [Then("the signature of the JWS payload is valid")]
        public void ThenTheSignatureIsCorrect()
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                var serializedRsa = _globalContext.FakeDataSource.JsonWebKeys.First().SerializedKey;
                provider.FromXmlString(serializedRsa);
                var signature = _signedPayLoad.Base64DecodeBytes();
                var payLoad = ASCIIEncoding.ASCII.GetBytes(_combinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.True(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public void ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.True(record.Alg.Equals(_jwsProtectedHeader.Alg));
        }

        [Then("the audience parameter with value (.*) is returned by the JWS payload")]
        public void ThenAudienceIsReturnedInJwsPayLoad(string audience)
        {
            Assert.True(_jwsPayLoad.Audiences.Contains(audience));
        }

        [Then("the parameter nonce with value (.*) is returned by the JWS payload")]
        public void ThenNonceIsReturnedInJwsPayLoad(string nonce)
        {
            Assert.True(_jwsPayLoad.Nonce.Equals(nonce));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public void ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var claimValue = _jwsPayLoad.GetClaimValue(claimName);

            Assert.NotNull(claimValue);
            Assert.True(claimValue.Equals(val));
        }

        [Then("the JWS payload contains (.*) claims")]
        public void ThenTheJwsPayloadContainsNumberOfClaims(int numberOfClaims)
        {
            Assert.True(_jwsPayLoad.Count.Equals(numberOfClaims));
        }

        [Then("the callback contains the following query name (.*)")]
        public void ThenTheCallbackContainsTheQueryName(string queryName)
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Fragment.TrimStart('#'));
            var queryValue = query[queryName];
            Assert.NotEmpty(queryValue);
        }

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
