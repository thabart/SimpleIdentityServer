using Microsoft.Owin.Testing;

using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Helpers;
using System.Net.Http;
using System.Text;
using TechTalk.SpecFlow;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding]
    public sealed class GetAccessTokenWithResourceOwnerGrantTypeSpec
    {
        private readonly ConfigureWebApi _configureWebApi;

        private readonly ISecurityHelper _securityHelper;

        private string _password;

        private string _userName;

        private string _clientId;

        public GetAccessTokenWithResourceOwnerGrantTypeSpec()
        {
            _configureWebApi = new ConfigureWebApi();
            _securityHelper = new SecurityHelper();
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            _userName = userName;
            _password = password;
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
            _clientId = clientId;
            var client = new Client
            {
                ClientId = clientId
            };

            _configureWebApi.DataSource.Clients.Add(client);
        }

        [When("requesting an access token via resource owner grant-type")]
        public void WhenRequestingAnAccessToken()
        {
            using (var server = TestServer.Create<Startup>())
            {
                var httpClient = server.HttpClient;
                var parameter = string.Format(
                    "grant_type=password&username={0}&password={1}&client_id={2}",
                    _userName,
                    _password,
                    _clientId);
                var content = new StringContent(parameter, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var token = httpClient.PostAsync("/api/token", content).Result;
                string s = "";
            }
        }
    }
}
