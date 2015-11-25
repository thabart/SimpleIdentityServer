using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.RateLimitation.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetUserInformation")]
    public sealed class GetUserInformationSpec
    {
        private HttpResponseMessage _authorizationResponseMessage;

        private HttpResponseMessage _userInformationResponseMessage;

        private JwsPayload _jwsPayload;

        private readonly GlobalContext _context;

        private static TestServer _testServer;

        private MODELS.ResourceOwner _resourceOwner;

        public GetUserInformationSpec(GlobalContext context)
        {
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };

            _context = context;
            _context.UnityContainer.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
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

        [Given("requesting an access token")]
        public void GivenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwner = _resourceOwner
            });
            _testServer = _context.CreateServer(httpConfiguration);
            var httpClient = _testServer.HttpClient;
            var url = string.Format(
                "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}",
                authorizationRequest.scope,
                authorizationRequest.response_type,
                authorizationRequest.client_id,
                authorizationRequest.redirect_uri,
                authorizationRequest.prompt,
                authorizationRequest.state,
                authorizationRequest.nonce);
            _authorizationResponseMessage = httpClient.GetAsync(url).Result;
        }

        [When("requesting user information")]
        public void WhenRequestingUserInformation()
        {
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Query);
            var accessToken = query["access_token"];
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwner = _resourceOwner
            });
            _testServer = _context.CreateServer(httpConfiguration);
            var httpClient = _testServer.HttpClient;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _userInformationResponseMessage = httpClient.GetAsync("/userinfo").Result;
            var r = _userInformationResponseMessage.Content.ReadAsStringAsync().Result;
            _jwsPayload = _userInformationResponseMessage.Content.ReadAsAsync<JwsPayload>().Result;
        }

        [Then("HTTP status code is (.*)")]
        public void ThenHttpStatusCodeIs(HttpStatusCode httpStatusCode)
        {
            Assert.That(_userInformationResponseMessage.StatusCode, Is.EqualTo(httpStatusCode));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public void ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var claimValue = _jwsPayload.GetClaimValue(claimName);

            Assert.IsNotNull(claimValue);
            Assert.That(claimValue, Is.EqualTo(val));
        }
    }
}
