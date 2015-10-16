using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using Microsoft.Practices.Unity;

using NUnit.Framework;

using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.RateLimitation.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenWithResourceOwnerGrantType")]
    public sealed class GetAccessTokenWithResourceOwnerGrantTypeSpec
    {
        private readonly IDataSource _dataSource;

        private readonly ConfigureWebApi _configureWebApi;

        private readonly ISecurityHelper _securityHelper;

        private GrantedToken _token;

        private ErrorResponse _errorResponse;

        private HttpStatusCode _httpStatusCode;

        public GetAccessTokenWithResourceOwnerGrantTypeSpec()
        {
            _dataSource = new FakeDataSource();
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };

            _configureWebApi = new ConfigureWebApi();
            _configureWebApi.Container.RegisterInstance<IDataSource>(_dataSource);
            _configureWebApi.Container.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
            _securityHelper = new SecurityHelper();
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            var resourceOwner = new ResourceOwner
            {
                Id = userName,
                Password = _securityHelper.ComputeHash(password)
            };

            _dataSource.ResourceOwners.Add(resourceOwner);
        }
        
        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new Client
            {
                ClientId = clientId,
                AllowedScopes = new List<Scope>()
            };

            _dataSource.Clients.Add(client);
        }

        [Given("scopes (.*) are defined")]
        public void GivenScope(List<string> scopes)
        {
            foreach (var scope in scopes) {
                var record = new Scope
                {
                    Name = scope
                };

                _dataSource.Scopes.Add(record);
            }
        }

        [Given("the scopes (.*) are assigned to the client (.*)")]
        public void GivenScopesToTheClients(List<string> scopeNames, string clientId)
        {
            var client = _dataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            if (client == null)
            {
                return;
            }

            var scopes = _dataSource.Scopes;
            foreach(var scopeName in scopeNames)
            {
                var storedScope = scopes.SingleOrDefault(s => s.Name == scopeName);
                if (storedScope == null)
                {
                    continue;
                }

                client.AllowedScopes.Add(storedScope);
            }
        }

        [When("requesting an access token via resource owner grant-type")]
        public void WhenRequestingAnAccessToken(Table table)
        {
            var tokenRequest = table.CreateInstance<TokenRequest>();
            using (var server = _configureWebApi.CreateServer())
            {
                var httpClient = server.HttpClient;
                var parameter = string.Format(
                    "grant_type=password&username={0}&password={1}&client_id={2}&scope={3}",
                    tokenRequest.username,
                    tokenRequest.password,
                    tokenRequest.client_id,
                    tokenRequest.scope);
                var content = new StringContent(parameter, Encoding.UTF8, "application/x-www-form-urlencoded");

                var result = httpClient.PostAsync("/api/token", content).Result;
                _httpStatusCode = result.StatusCode;
                if (_httpStatusCode == HttpStatusCode.OK)
                {
                    _token = result.Content.ReadAsAsync<GrantedToken>().Result;
                    return;
                }

                _errorResponse = result.Content.ReadAsAsync<ErrorResponse>().Result;
            }
        }

        [Then("http result is (.*)")]
        public void ThenHttpResultIsCorrect(HttpStatusCode code)
        {
            Assert.That(_httpStatusCode, Is.EqualTo(code));
        }

        [Then("access token is generated")]
        public void ThenAccessTokenIsGenerated()
        {
            Assert.IsNotNullOrEmpty(_token.AccessToken);
        }

        [Then("access token have the correct scopes : (.*)")]
        public void ThenAccessTokenContainsCorrectScopes(List<string> scopes)
        {
            var accessTokenScopes = _token.Scope.Split(' ');
            var atLeastOneScopeIsNotTheSame =
                scopes.Where(a => !string.IsNullOrEmpty(a) && !accessTokenScopes.Contains(a))
                .Count() > 0;

            Assert.IsFalse(atLeastOneScopeIsNotTheSame);
        }

        [Then("the error is (.*)")]
        public void ThenErrorCodeIsCorrect(string errorCode)
        {
            Assert.That(_errorResponse.error, Is.EqualTo(errorCode));
        }
    }
}
