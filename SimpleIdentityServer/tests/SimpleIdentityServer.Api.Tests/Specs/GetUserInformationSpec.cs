using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.RateLimitation.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Common.Extensions;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.Core.Helpers;
using System.Security.Cryptography;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using System.Linq;
using System;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetUserInformation")]
    public sealed class GetUserInformationSpec
    {
        private ISecurityHelper _securityHelper;
        
        private HttpResponseMessage _authorizationResponseMessage;

        private HttpResponseMessage _userInformationResponseMessage;

        private FakeSimpleIdentityServerConfigurator _fakeSimpleIdentityServerConfigurator;

        private JwsPayload _jwsPayload;

        private GlobalContext _globalContext;

        private MODELS.ResourceOwner _resourceOwner;
        
        [BeforeScenario]
        private void Init() 
        {            
            _securityHelper = new SecurityHelper();
            _fakeSimpleIdentityServerConfigurator = new FakeSimpleIdentityServerConfigurator();
            _globalContext = new GlobalContext();
            _globalContext.Init();
            _globalContext.CreateServer(services => {
                var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
                {
                    Enabled = false
                };
               services.AddInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
               services.AddInstance<ISimpleIdentityServerConfigurator>(_fakeSimpleIdentityServerConfigurator);
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

        private MODELS.Client GetClient(string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
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

        [Given("set the name of the issuer (.*)")]
        public void GivenIssuerName(string issuerName)
        {
            _fakeSimpleIdentityServerConfigurator.Issuer = issuerName;
        }

        [Given("requesting an access token")]
        public void GivenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwner = _resourceOwner
            });
            var httpClient = _globalContext.TestServer.CreateClient();
            var url = string.Format(
                "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}&claims={7}",
                authorizationRequest.scope,
                authorizationRequest.response_type,
                authorizationRequest.client_id,
                authorizationRequest.redirect_uri,
                authorizationRequest.prompt,
                authorizationRequest.state,
                authorizationRequest.nonce,
                authorizationRequest.claims);
            _authorizationResponseMessage = httpClient.GetAsync(url).Result;
        }

        [When("requesting user information and the access token is passed to the authorization header")]
        public void WhenRequestingUserInformationAndPassedTheAccessTokenToAuthorizationHeader()
        {
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Fragment.TrimStart('#'));
            var accessToken = query["access_token"];
            var httpConfiguration = new HttpConfiguration();
            var httpClient = _globalContext.TestServer.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _userInformationResponseMessage = httpClient.GetAsync("/userinfo").Result;
            var r = _userInformationResponseMessage.Content.ReadAsStringAsync().Result;
            _jwsPayload = _userInformationResponseMessage.Content.ReadAsAsync<JwsPayload>().Result;
        }

        [When("requesting user information and the access token is passed to the HTTP body")]
        public void WhenRequestingUserInformationAndPassedTheAccessTokenToHttpBody()
        {
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Fragment.TrimStart('#'));
            var accessToken = query["access_token"];
            var httpConfiguration = new HttpConfiguration();
            var httpClient = _globalContext.TestServer.CreateClient();
            var parameter = new Dictionary<string, string>
            {
                {
                    "access_token",
                    accessToken
                }
            };
            var encodedParameter = new FormUrlEncodedContent(parameter);
            _userInformationResponseMessage = httpClient.PostAsync("/userinfo", encodedParameter).Result;
            var r = _userInformationResponseMessage.Content.ReadAsStringAsync().Result;
            _jwsPayload = _userInformationResponseMessage.Content.ReadAsAsync<JwsPayload>().Result;
        }

        [When("requesting user information and the access token is passed in the query string")]
        public void WhenRequestingUserInformationAndPassedTheAccessTokenInTheQueryString()
        {
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Fragment.TrimStart('#'));
            var accessToken = query["access_token"];
            var httpClient = _globalContext.TestServer.CreateClient();
            _userInformationResponseMessage = httpClient
                .GetAsync(string.Format("/userinfo?access_token={0}", accessToken))
                .Result;
            var r = _userInformationResponseMessage.Content.ReadAsStringAsync().Result;
            _jwsPayload = _userInformationResponseMessage.Content.ReadAsAsync<JwsPayload>().Result;
        }

        [Then("HTTP status code is (.*)")]
        public void ThenHttpStatusCodeIs(HttpStatusCode httpStatusCode)
        {
            Assert.True(_userInformationResponseMessage.StatusCode.Equals(httpStatusCode));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public void ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var claimValue = _jwsPayload.GetClaimValue(claimName);

            Assert.NotNull(claimValue);
            Assert.True(claimValue.Equals(val));
        }

        [Then("the JWS payload contains (.*) claims")]
        public void ThenTheJwsPayloadContainsNumberOfClaims(int numberOfClaims)
        {
            Assert.True(_jwsPayload.Count.Equals(numberOfClaims));
        }

        [Then("the returned address is")]
        public void ThenTheReturnedAddressIs(Table table)
        {
            var address = table.CreateInstance<MODELS.Address>();
            var claimValue = _jwsPayload.GetClaimValue("address");
            Assert.NotNull(claimValue);
            var receivedAddress = claimValue.DeserializeWithDataContract<MODELS.Address>();
            Assert.NotNull(receivedAddress);

            Assert.True(receivedAddress.Country.Equals(address.Country));
            Assert.True(receivedAddress.Formatted.Equals(address.Formatted));
            Assert.True(receivedAddress.Locality.Equals(address.Locality));
            Assert.True(receivedAddress.Region.Equals(address.Region));
            Assert.True(receivedAddress.StreetAddress.Equals(address.StreetAddress));
        }
    }
}
