using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
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

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAuthorizationCode")]
    public class GetAuthorizationCodeSpec
    {
        private readonly ConfigureWebApi _configureWebApi;

        private HttpResponseMessage _responseMessage;

        private FakeUserInformation _fakeUserInformation;

        public GetAuthorizationCodeSpec()
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
            var client = FakeDataSource.Instance().Clients.SingleOrDefault(c => c.ClientId == clientId);
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

        [Given("a resource owner is authenticated")]
        public void GivenAResourceOwnerIsAuthenticated(Table table)
        {
            _fakeUserInformation = table.CreateInstance<FakeUserInformation>();
        }

        [When("requesting an authorization code")]
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
                var url = string.Format("/api/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}",
                    authorizationRequest.scope,
                    authorizationRequest.response_type,
                    authorizationRequest.client_id,
                    authorizationRequest.redirect_uri,
                    authorizationRequest.prompt);
                _responseMessage = httpClient.GetAsync(url).Result;
            }
        }

        [Then("HTTP status code is (.*)")]
        public void ThenHttpStatusCodeIs(HttpStatusCode httpStatusCode)
        {
            Assert.That(_responseMessage.StatusCode, Is.EqualTo(httpStatusCode));
        }

        [Then("redirect to (.*) controller")]
        public void ThenRedirectToController(string controller)
        {
            var location = _responseMessage.Headers.Location;
            Assert.That(location.AbsolutePath, Is.EqualTo(controller));
        }
    }
}
