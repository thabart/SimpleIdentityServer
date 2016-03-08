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
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.RateLimitation.Configuration;
using SimpleIdentityServer.RateLimitation.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenMultipleTime")]
    public sealed class GetAccessTokenMultipleTimeSpec
    {
        private class TokenResponse
        {
            public List<DOMAINS.GrantedToken> Tokens { get; set; }

            public List<FakeTooManyRequestResponse> Errors { get; set; }

            public List<FakeHttpResponse> HttpResponses { get; set; }
        }

        private GlobalContext _globalContext;
        
        private ISecurityHelper _securityHelper;
        
        private RateLimitationElement _rateLimitationElement;

        private Func<Task<TokenResponse>> _getTokensCallback;
        
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

        #endregion

        #region When

        [When("requesting access tokens")]
        public void WhenRequestingAccessTokens(Table table)
        {
            var tokenRequests = table.CreateSet<TokenRequest>();

            // TODO : flush memory cache :(
            var responseCacheManager = _globalContext.MemoryCache;
            // responseCacheManager.Flush();

            _getTokensCallback = async () =>
            {
                var res = new TokenResponse
                {
                    Errors = new List<FakeTooManyRequestResponse>(),
                    Tokens = new List<Core.Models.GrantedToken>()
                };
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

                    var result = await httpClient.PostAsync("/token", content).ConfigureAwait(false);
                    var httpStatusCode = result.StatusCode;
                    res.HttpResponses.Add(new FakeHttpResponse
                    {
                        StatusCode = httpStatusCode,
                        NumberOfRequests = result.Headers.GetValues(RateLimitationConstants.XRateLimitLimitName).FirstOrDefault(),
                        NumberOfRemainingRequests = result.Headers.GetValues(RateLimitationConstants.XRateLimitRemainingName).FirstOrDefault()
                    });
                    if (httpStatusCode == HttpStatusCode.OK)
                    {
                        res.Tokens.Add(result.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result);
                        continue;
                    }

                    res.Errors.Add(new FakeTooManyRequestResponse
                    {
                        Message = result.Content.ReadAsStringAsync().Result,
                    });
                }

                return res;
            };
            
        }

        #endregion

        #region Then

        [Then("(.*) access tokens are generated")]
        public async Task ThenTheResultShouldBe(int numberOfAccessTokens)
        {
            var response = await _getTokensCallback();
            Assert.True(response.Tokens.Count.Equals(numberOfAccessTokens));
        }

        [Then("the errors should be returned")]
        public async Task ThenErrorsShouldBe(Table table)
        {
            var response = await _getTokensCallback();
            var records = table.CreateSet<FakeTooManyRequestResponse>().ToList();
            Assert.True(records.Count.Equals(response.Errors.Count()));
            for (var i = 0; i < records.Count() - 1; i++)
            {
                var record = records[i];
                var error = response.Errors[i];
                Assert.True(record.Message.Equals(error.Message));
            }
        }

        [Then("the http responses should be returned")]
        public async Task ThenHttpHeadersShouldContain(Table table)
        {
            var response = await _getTokensCallback();
            var records = table.CreateSet<FakeHttpResponse>().ToList();
            Assert.True(records.Count.Equals(response.HttpResponses.Count()));
            for(var i = 0; i < records.Count() - 1; i++)
            {
                var record = records[i];
                var httpResponse = response.HttpResponses[i];
                Assert.True(record.StatusCode.Equals(record.StatusCode));
                Assert.True(record.NumberOfRemainingRequests.Equals(record.NumberOfRemainingRequests));
                Assert.True(record.NumberOfRequests.Equals(record.NumberOfRequests));
            }
        }

        #endregion

        [AfterScenario]
        public void After() 
        {
            Console.WriteLine("dispose");
            _globalContext.TestServer.Dispose();
        }

        private MODELS.Client GetClient(string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }
    }
}
