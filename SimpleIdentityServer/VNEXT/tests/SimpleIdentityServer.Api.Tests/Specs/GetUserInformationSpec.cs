using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Microsoft.Practices.Unity;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.RateLimitation.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Common.Extensions;
using System.Collections.Generic;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetUserInformation")]
    public sealed class GetUserInformationSpec
    {
        private HttpResponseMessage _authorizationResponseMessage;

        private HttpResponseMessage _userInformationResponseMessage;

        private readonly FakeSimpleIdentityServerConfigurator _fakeSimpleIdentityServerConfigurator;

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
            _fakeSimpleIdentityServerConfigurator = new FakeSimpleIdentityServerConfigurator(); ;

            _context = context;
            _context.UnityContainer.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
            _context.UnityContainer.RegisterInstance<ISimpleIdentityServerConfigurator>(_fakeSimpleIdentityServerConfigurator);
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
            _testServer = _context.CreateServer(httpConfiguration);
            var httpClient = _testServer.HttpClient;
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

        [When("requesting user information and the access token is passed to the HTTP body")]
        public void WhenRequestingUserInformationAndPassedTheAccessTokenToHttpBody()
        {
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Fragment.TrimStart('#'));
            var accessToken = query["access_token"];
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwner = _resourceOwner
            });
            _testServer = _context.CreateServer(httpConfiguration);
            var httpClient = _testServer.HttpClient;
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
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwner = _resourceOwner
            });
            _testServer = _context.CreateServer(httpConfiguration);
            var httpClient = _testServer.HttpClient;
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
