﻿using Microsoft.Owin.Testing;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using NUnit.Framework;
using RateLimitation.Configuration;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenMultipleTime")]
    public sealed class GetAccessTokenMultipleTimeSpec
    {
        private readonly ConfigureWebApi _configureWebApi;

        private readonly ISecurityHelper _securityHelper;

        private List<GrantedToken> _tokens;

        private List<TooManyRequestResponse> _errors;

        private int _numberOfRequest;

        private RateLimitationElement _rateLimitationElement;

        private HttpStatusCode _httpStatusCode;

        public GetAccessTokenMultipleTimeSpec()
        {
            _rateLimitationElement = new RateLimitationElement
            {
                Name = "PostToken"
            };
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = true,
                RateLimitationElement = _rateLimitationElement
            };
            _configureWebApi = new ConfigureWebApi(fakeGetRateLimitationElementOperation);
            _securityHelper = new SecurityHelper();
            _tokens = new List<GrantedToken>();
            _errors = new List<TooManyRequestResponse>();
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            var resourceOwner = new ResourceOwner
            {
                Id = userName,
                Password = _securityHelper.ComputeHash(password)
            };

            _configureWebApi.DataSource.ResourceOwners.Add(resourceOwner);
        }

        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new Client
            {
                ClientId = clientId,
                AllowedScopes = new List<Scope>()
            };

            _configureWebApi.DataSource.Clients.Add(client);
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
            var responseCacheManager = CacheFactory.GetCacheManager();
            responseCacheManager.Flush();
            using (var server = TestServer.Create<Startup>())
            {
                foreach (var tokenRequest in tokenRequests)
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
                        _tokens.Add(result.Content.ReadAsAsync<GrantedToken>().Result);
                        continue;
                    }

                    _errors.Add(new TooManyRequestResponse
                    {
                        HttpStatusCode = _httpStatusCode,
                        Message = result.Content.ReadAsAsync<string>().Result
                    });
                }
            }
        }

        [Then("(.*) access tokens are generated")]
        public void ThenTheResultShouldBe(int numberOfAccessTokens)
        {
            Assert.That(_tokens.Count, Is.EqualTo(numberOfAccessTokens));
        }

        [Then("the errors should be returned")]
        public void ThenErrorsShouldBe(Table table)
        {
            var records = table.CreateSet<TooManyRequestResponse>();
            foreach(var record in records)
            {
                Assert.IsTrue(_errors.Any(e => e.HttpStatusCode == record.HttpStatusCode && e.Message == record.Message));
            }
        }
    }
}
