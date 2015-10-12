using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Helpers;
using System.Collections.Generic;
using System.Net;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetAccessTokenMultipleTime")]
    public sealed class GetAccessTokenMultipleTimeSpec
    {
        private readonly ConfigureWebApi _configureWebApi;

        private readonly ISecurityHelper _securityHelper;

        private GrantedToken _token;

        private ErrorResponse _errorResponse;

        private HttpStatusCode _httpStatusCode;

        public GetAccessTokenMultipleTimeSpec()
        {
            _configureWebApi = new ConfigureWebApi();
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

        [When("requesting an access token (.*) times")]
        public void ThenTheResultShouldBe(int result, Table table)
        {
            var tokenRequest = table.CreateInstance<TokenRequest>();
        }

        [Then("the result should be")]
        public void ThenTheResultShouldBe(Table table)
        {

        }
    }
}
