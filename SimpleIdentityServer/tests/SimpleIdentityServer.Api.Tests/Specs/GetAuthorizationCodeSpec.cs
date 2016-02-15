using System;
using System.Net;
using System.Net.Http;

using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.RateLimitation.Configuration;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using System.Web;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Helpers;
using System.Security.Cryptography;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using System.Linq;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAuthorizationCode")]
    public class GetAuthorizationCodeSpec
    {
        private ISecurityHelper _securityHelper;
        
        private GlobalContext _globalContext;

        private HttpResponseMessage _responseMessage;

        private MODELS.ResourceOwner _resourceOwner;
        
        [BeforeScenario]
        private void Init()
        {            
            _securityHelper = new SecurityHelper();
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };

            _globalContext = new GlobalContext();
            _globalContext.Init();
            _globalContext.CreateServer(services => {                
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

        [When("requesting an authorization code")]
        public void WhenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var responseModeName = Enum.GetName(typeof (ResponseMode), authorizationRequest.response_mode);
            var client = _globalContext.TestServer.CreateClient();
            var url = string.Format(
                    "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&response_mode={6}",
                    authorizationRequest.scope,
                    authorizationRequest.response_type,
                    authorizationRequest.client_id,
                    authorizationRequest.redirect_uri,
                    authorizationRequest.prompt,
                    authorizationRequest.state,
                    responseModeName);
            _responseMessage = client.GetAsync(url).Result;
        }

        [Then("HTTP status code is (.*)")]
        public void ThenHttpStatusCodeIs(HttpStatusCode httpStatusCode)
        {
            Assert.True(_responseMessage.StatusCode.Equals(httpStatusCode));
        }

        [Then("redirect to (.*) controller")]
        public void ThenRedirectToController(string controller)
        {
            var location = _responseMessage.Headers.Location;
            Assert.True(location.AbsolutePath.Equals(controller));
        }

        [Then("redirect to callback (.*)")]
        public void ThenRedirectToCallback(string callback)
        {
            var location = _responseMessage.Headers.Location;
            Assert.True(location.AbsoluteUri.StartsWith(callback));
        }

        [Then("the error returned is")]
        public void ThenTheErrorReturnedIs(Table table)
        {
            var errorResponseWithState = table.CreateInstance<ErrorResponseWithState>();
            var result = _responseMessage.Content.ReadAsAsync<ErrorResponseWithState>().Result;

            Assert.True(errorResponseWithState.error.Equals(result.error));
            Assert.True(errorResponseWithState.state.Equals(result.state));
        }

        [Then("the query string (.*) with value (.*) is returned")]
        public void ThenTheQueryStringIsContained(string queryName, string queryValue)
        {
            var location = _responseMessage.Headers.Location;
            var queries = HttpUtility.ParseQueryString(location.Query);

            var value = queries[queryName];
            Assert.NotNull(value);
            Assert.Equal(value, queryValue);
        }

        [Then("the query string (.*) exists")]
        public void ThenTheQueryStringExists(string queryName)
        {
            var location = _responseMessage.Headers.Location;
            var queries = HttpUtility.ParseQueryString(location.Query);
            var value = queries[queryName];
            Assert.NotNull(value);
        }

        [Then("the fragment contains the query (.*) with the value (.*)")]
        public void ThenFragmentContainsTheQueryWithValue(string queryName, string queryValue)
        {
            var fragment = _responseMessage.Headers.Location.Fragment;
            Assert.NotEmpty(fragment);
            fragment = fragment.TrimStart('#');
            var queries = HttpUtility.ParseQueryString(fragment);
            var value = queries[queryName];
            Assert.NotNull(value);
            Assert.Equal(value, queryValue);
        }

        [Then("the fragment contains the query string (.*)")]
        public void ThenFragmentsContainsTheQueryString(string queryName)
        {
            var fragment = _responseMessage.Headers.Location.Fragment;
            Assert.NotEmpty(fragment);
            fragment = fragment.TrimStart('#');
            var queries = HttpUtility.ParseQueryString(fragment);
            var value = queries[queryName];
            Assert.NotNull(value);
        }
        
        [AfterScenario]
        public void After() 
        {
            Console.WriteLine("dispose");
            _globalContext.TestServer.Dispose();
        }
    }
}
