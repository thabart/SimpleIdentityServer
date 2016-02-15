using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.RateLimitation.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core.Helpers;
using System.Security.Cryptography;
using SimpleIdentityServer.Api.Tests.Extensions;
using System;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenMultipleTime")]
    public sealed class GetAccessTokenMultipleTimeSpec
    {
        private GlobalContext _globalContext;
        
        private ISecurityHelper _securityHelper;

        private List<DOMAINS.GrantedToken> _tokens;

        private List<FakeTooManyRequestResponse> _errors;

        private RateLimitationElement _rateLimitationElement;

        private List<FakeHttpResponse> _httpResponses;
        
        [BeforeScenario]
        private void Init() 
        {            
            _securityHelper = new SecurityHelper();
            _globalContext = new GlobalContext();
            _globalContext.Init();

            _rateLimitationElement = new RateLimitationElement
            {
                Name = "PostToken"
            };
            
            _tokens = new List<DOMAINS.GrantedToken>();
            _errors = new List<FakeTooManyRequestResponse>();
            _httpResponses = new List<FakeHttpResponse>();

            _globalContext.CreateServer(services =>
            {
                var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
                {
                    Enabled = true,
                    RateLimitationElement = _rateLimitationElement
                };
                services.AddInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
                services.AddTransient<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
            });
            _globalContext.AuthenticationMiddleWareOptions.IsEnabled = true;
            _globalContext.AuthenticationMiddleWareOptions.ResourceOwner = new DataAccess.Fake.Models.ResourceOwner
            {
                Id = "subject"
            };
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

        [Given("allowed number of requests is (.*)")]
        public void GivenAllowedNumberOfRequests(int numberOfRequests)
        {
            _rateLimitationElement.NumberOfRequests = numberOfRequests;
        }

        [Given("sliding time is (.*)")]
        public void GivenSlidingTime(double slidingTime)
        {
            _rateLimitationElement.SlidingTime = slidingTime;
        }

        [When("requesting access tokens")]
        public void WhenRequestingAccessTokens(Table table)
        {
            var tokenRequests = table.CreateSet<TokenRequest>();
            var responseCacheManager = _globalContext.ServiceProvider.GetService<ICacheManagerProvider>().GetCacheManager();
            responseCacheManager.Flush();

            var httpClient = _globalContext.TestServer.CreateClient();
            foreach (var tokenRequest in tokenRequests)
            {
                var parameter = string.Format(
                    "grant_type=password&username={0}&password={1}&client_id={2}&scope={3}",
                    tokenRequest.username,
                    tokenRequest.password,
                    tokenRequest.client_id,
                    tokenRequest.scope);
                var content = new StringContent(parameter, Encoding.UTF8, "application/x-www-form-urlencoded");
            
                var result = httpClient.PostAsync("/token", content).Result;
                var httpStatusCode = result.StatusCode;
                _httpResponses.Add(new FakeHttpResponse
                {
                    StatusCode = httpStatusCode,
                    NumberOfRequests = result.Headers.GetValues(RateLimitationConstants.XRateLimitLimitName).FirstOrDefault(),
                    NumberOfRemainingRequests = result.Headers.GetValues(RateLimitationConstants.XRateLimitRemainingName).FirstOrDefault()
                });
                if (httpStatusCode == HttpStatusCode.OK)
                {
                    _tokens.Add(result.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result);
                    continue;
                }
                
                _errors.Add(new FakeTooManyRequestResponse
                {
                    Message = result.Content.ReadAsStringAsync().Result,
                });
            }
        }

        [Then("(.*) access tokens are generated")]
        public void ThenTheResultShouldBe(int numberOfAccessTokens)
        {
            Assert.True(_tokens.Count.Equals(numberOfAccessTokens));
        }

        [Then("the errors should be returned")]
        public void ThenErrorsShouldBe(Table table)
        {
            var records = table.CreateSet<FakeTooManyRequestResponse>().ToList();
            Assert.True(records.Count.Equals(_errors.Count()));
            for (var i = 0; i < records.Count() - 1; i++)
            {
                var record = records[i];
                var error = _errors[i];
                Assert.True(record.Message.Equals(error.Message));
            }
        }

        [Then("the http responses should be returned")]
        public void ThenHttpHeadersShouldContain(Table table)
        {
            var records = table.CreateSet<FakeHttpResponse>().ToList();
            Assert.True(records.Count.Equals(_httpResponses.Count()));
            for(var i = 0; i < records.Count() - 1; i++)
            {
                var record = records[i];
                var httpResponse = _httpResponses[i];
                Assert.True(record.StatusCode.Equals(record.StatusCode));
                Assert.True(record.NumberOfRemainingRequests.Equals(record.NumberOfRemainingRequests));
                Assert.True(record.NumberOfRequests.Equals(record.NumberOfRequests));
            }
        }
        
        [AfterScenario]
        public void After() 
        {
            Console.WriteLine("dispose");
            _globalContext.TestServer.Dispose();
        }
    }
}
