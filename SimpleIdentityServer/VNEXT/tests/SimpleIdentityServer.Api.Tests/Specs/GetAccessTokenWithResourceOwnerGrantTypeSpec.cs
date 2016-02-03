using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.RateLimitation.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using DOMAINS = SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenWithResourceOwnerGrantType")]
    public sealed class GetAccessTokenWithResourceOwnerGrantTypeSpec
    {
        private readonly GlobalContext _globalContext;

        private DOMAINS.GrantedToken _token;

        private ErrorResponse _errorResponse;

        private HttpStatusCode _httpStatusCode;

        public GetAccessTokenWithResourceOwnerGrantTypeSpec(GlobalContext globalContext)
        {
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };

            _globalContext = globalContext;
            globalContext.CreateServer(services => {                
                services.AddInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
                services.AddTransient<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
            });
        }

        [When("requesting an access token via resource owner grant-type")]
        public void WhenRequestingAnAccessToken(Table table)
        {
            var tokenRequest = table.CreateInstance<TokenRequest>();
            var httpClient = _globalContext.TestServer.CreateClient();
            var parameter = string.Format(
                "grant_type=password&username={0}&password={1}&client_id={2}&scope={3}",
                tokenRequest.username,
                tokenRequest.password,
                tokenRequest.client_id,
                tokenRequest.scope);
            var content = new StringContent(parameter, Encoding.UTF8, "application/x-www-form-urlencoded");

            var result = httpClient.PostAsync("/token", content).Result;
            _httpStatusCode = result.StatusCode;
            if (_httpStatusCode == HttpStatusCode.OK)
            {
                _token = result.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result;
                return;
            }

            _errorResponse = result.Content.ReadAsAsync<ErrorResponse>().Result;
        }

        [Then("http result is (.*)")]
        public void ThenHttpResultIsCorrect(HttpStatusCode code)
        {
            Assert.True(_httpStatusCode.Equals(code));
        }

        [Then("access token is generated")]
        public void ThenAccessTokenIsGenerated()
        {
            Assert.NotEmpty(_token.AccessToken);
        }

        [Then("access token have the correct scopes : (.*)")]
        public void ThenAccessTokenContainsCorrectScopes(List<string> scopes)
        {
            var accessTokenScopes = _token.Scope.Split(' ');
            var atLeastOneScopeIsNotTheSame =
                scopes.Where(a => !string.IsNullOrEmpty(a) && !accessTokenScopes.Contains(a))
                .Count() > 0;

            Assert.False(atLeastOneScopeIsNotTheSame);
        }

        [Then("the error is (.*)")]
        public void ThenErrorCodeIsCorrect(string errorCode)
        {
            Assert.True(_errorResponse.error.Equals(errorCode));
        }
    }
}
