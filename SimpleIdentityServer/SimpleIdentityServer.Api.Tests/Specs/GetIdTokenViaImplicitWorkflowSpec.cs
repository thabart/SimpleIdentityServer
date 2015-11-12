using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

using Microsoft.Practices.Unity;

using NUnit.Framework;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.RateLimitation.Configuration;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using System.Web;
using SimpleIdentityServer.Core.Extensions;
using System.Web.Script.Serialization;
using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetIdTokenViaImplicitWorkflow")]
    public sealed class GetIdTokenViaImplicitWorkflowSpec
    {
        private readonly ConfigureWebApi _configureWebApi;

        private HttpResponseMessage _httpResponseMessage;

        private FakeUserInformation _fakeUserInformation;

        private JwsProtectedHeader _jwsProtectedHeader;

        private JwsPayload _jwsPayLoad;

        private string _signedPayLoad;

        private string _serializedRsa;

        private string _combinedHeaderAndPayload;

        public GetIdTokenViaImplicitWorkflowSpec()
        {
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };
            
            _configureWebApi = new ConfigureWebApi();
            _configureWebApi.Container.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
        }

        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new MODELS.Client
            {
                ClientId = clientId,
                AllowedScopes = new List<MODELS.Scope>()
            };

            FakeDataSource.Instance().Clients.Add(client);
        }

        [Given("create a RSA key")]
        public void GivenCreateRsaKey()
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                _serializedRsa = provider.ToXmlString(true);
            }

            FakeDataSource.Instance().JsonWebKeys.Add(new MODELS.JsonWebKey
            {
                Alg = MODELS.AllAlg.RS256,
                KeyOps = new[]
                    {
                        MODELS.KeyOperations.Sign
                    },
                Kid = "1",
                Kty = MODELS.KeyType.RSA,
                SerializedKey = _serializedRsa
            });
        }
        
        [Given("scopes (.*) are defined")]
        public void GivenScope(List<string> scopes)
        {
            foreach (var scope in scopes)
            {
                var record = new MODELS.Scope
                {
                    Name = scope
                };

                FakeDataSource.Instance().Scopes.Add(record);
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

            var scopes = FakeDataSource.Instance().Scopes;
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

        [Given("the id_token signature algorithm is set to (.*) for the client (.*)")]
        public void GivenIdTokenSignatureAlgorithmIsSetForTheClient(string algorithm, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenSignedTResponseAlg = algorithm;
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
                var resp = (MODELS.ResponseType)Enum.Parse(typeof (ResponseType), responseType);
                client.ResponseTypes.Add(resp);
            }
        }

        [Given("a resource owner is authenticated")]
        public void GivenAResourceOwnerIsAuthenticated(Table table)
        {
            _fakeUserInformation = table.CreateInstance<FakeUserInformation>();
            var resourceOwner = new MODELS.ResourceOwner
            {
                Id = _fakeUserInformation.UserId
            };
            FakeDataSource.Instance().ResourceOwners.Add(resourceOwner);
        }
        
        [Given("the consent has been given by the resource owner (.*) for the client (.*) and scopes (.*)")]

        public void GivenConsent(string resourceOwnerId, string clientId, List<string> scopeNames)
        {
            var client = FakeDataSource.Instance().Clients.SingleOrDefault(c => c.ClientId == clientId);
            var resourceOwner = FakeDataSource.Instance().ResourceOwners.SingleOrDefault(r => r.Id == resourceOwnerId);
            var scopes = new List<MODELS.Scope>();
            foreach (var scopeName in scopeNames)
            {
                var storedScope = FakeDataSource.Instance().Scopes.SingleOrDefault(s => s.Name == scopeName);
                scopes.Add(storedScope);
            }
            var consent = new MODELS.Consent
            {
                Client = client,
                GrantedScopes = scopes,
                ResourceOwner = resourceOwner
            };

            FakeDataSource.Instance().Consents.Add(consent);
        }

        [When("requesting an authorization")]
        public void WhenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            // Fake the authentication filter.
            var httpConfiguration = new HttpConfiguration();
            if (_fakeUserInformation != null)
            {
                httpConfiguration.Filters.Add(new FakeAuthenticationFilter
                {
                    ResourceOwnerId = _fakeUserInformation.UserId,
                    ResourceOwnerUserName = _fakeUserInformation.UserName
                });
            }

            using (var server = _configureWebApi.CreateServer(httpConfiguration))
            {
                var httpClient = server.HttpClient;
                var url = string.Format(
                    "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}",
                    authorizationRequest.scope,
                    authorizationRequest.response_type,
                    authorizationRequest.client_id,
                    authorizationRequest.redirect_uri,
                    authorizationRequest.prompt,
                    authorizationRequest.state,
                    authorizationRequest.nonce);
                _httpResponseMessage = httpClient.GetAsync(url).Result;
            }
        }
        
        [Then("the http status code is (.*)")]
        public void ThenHttpStatusCodeIsCorrect(HttpStatusCode code)
        {
            Assert.That(code, Is.EqualTo(_httpResponseMessage.StatusCode));
        }

        [Then("redirect to (.*) controller")]
        public void ThenRedirectToController(string controller)
        {
            var location = _httpResponseMessage.Headers.Location;
            Assert.That(location.AbsolutePath, Is.EqualTo(controller));
        }

        [Then("decrypt the id_token parameter from the query string")]
        public void ThenDecryptTheIdTokenFromTheQueryString()
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Query);
            var idToken = query["id_token"];

            Assert.IsNotNull(idToken);

            var parts = idToken.Split('.');

            Assert.That(parts.Count() >= 3, Is.True);

            var secondPart = parts[1].Base64Decode();

            var javascriptSerializer = new JavaScriptSerializer();
            _jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
            _jwsPayLoad = javascriptSerializer.Deserialize<JwsPayload>(secondPart);
            _combinedHeaderAndPayload = parts[0] + "." + parts[1];
            _signedPayLoad = parts[2];
        }

        [Then("the signature of the JWS payload is valid")]
        public void ThenTheSignatureIsCorrect()
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(_serializedRsa);
                _signedPayLoad = _signedPayLoad.Replace(" ", "+");
                var signature = Convert.FromBase64String(_signedPayLoad);
                var payLoad = ASCIIEncoding.ASCII.GetBytes(_combinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.IsTrue(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public void ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.That(record.alg, Is.EqualTo(_jwsProtectedHeader.alg));
        }

        [Then("the audience parameter with value (.*) is returned by the JWS payload")]
        public void ThenAudienceIsReturnedInJwsPayLoad(string audience)
        {
            Assert.That(_jwsPayLoad.Audiences.Contains(audience), Is.True);
        }

        [Then("the parameter nonce with value (.*) is returned by the JWS payload")]
        public void ThenNonceIsReturnedInJwsPayLoad(string nonce)
        {
            Assert.That(_jwsPayLoad.Nonce, Is.EqualTo(nonce));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public void ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var claimValue = _jwsPayLoad.GetClaimValue(claimName);

            Assert.IsNotNull(claimValue);
            Assert.That(claimValue, Is.EqualTo(val));
        }

        [Then("the callback contains the following query name (.*)")]
        public void ThenTheCallbackContainsTheQueryName(string queryName)
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Query);
            var queryValue = query[queryName];
            Assert.IsNotNullOrEmpty(queryValue);
        }

        private static MODELS.Client GetClient(string clientId)
        {
            var client = FakeDataSource.Instance().Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }
    }
}
