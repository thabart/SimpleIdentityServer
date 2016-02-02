using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Microsoft.Practices.Unity;

using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.RateLimitation.Configuration;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using System.Web;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Configuration;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAuthorizationCode")]
    public class GetAuthorizationCodeSpec
    {
        private readonly GlobalContext _context;

        private HttpResponseMessage _responseMessage;

        private MODELS.ResourceOwner _resourceOwner;

        public GetAuthorizationCodeSpec(GlobalContext context)
        {
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };

            _context = context;
            _context.UnityContainer.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
            _context.UnityContainer.RegisterType<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
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
            FakeDataSource.Instance().ResourceOwners.Add(_resourceOwner);
        }

        [When("requesting an authorization code")]
        public void WhenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            // Fake the authentication filter.
            var httpConfiguration = new HttpConfiguration();
            if (_resourceOwner != null)
            {
                httpConfiguration.Filters.Add(new FakeAuthenticationFilter
                {
                    ResourceOwner = _resourceOwner
                });
            }

            var responseModeName = Enum.GetName(typeof (ResponseMode), authorizationRequest.response_mode);

            using (var server = _context.CreateServer(httpConfiguration))
            {
                var httpClient = server.HttpClient;
                var url = string.Format(
                    "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&response_mode={6}",
                    authorizationRequest.scope,
                    authorizationRequest.response_type,
                    authorizationRequest.client_id,
                    authorizationRequest.redirect_uri,
                    authorizationRequest.prompt,
                    authorizationRequest.state,
                    responseModeName);
                _responseMessage = httpClient.GetAsync(url).Result;
            }
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
    }
}
