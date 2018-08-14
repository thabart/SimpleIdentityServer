using Moq;
using SimpleIdentityServer.AccountFilter.Basic.Client;
using SimpleIdentityServer.AccountFilter.Basic.Client.Operations;
using SimpleIdentityServer.AccountFilter.Basic.Common.Requests;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.Common.Client.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests.Apis
{
    public class AccountFilterClientFixture : IClassFixture<TestOauthServerFixture>
    {
        const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IFilterClient _filterClient;
        private IClientAuthSelector _clientAuthSelector;

        public AccountFilterClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Get

        [Fact]
        public async Task When_Get_Filter_And_Doesnt_Exist_Then_Not_Found_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _filterClient.Get(baseUrl + "/filters", "filter_id", grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.True(result.ContainsError);
            Assert.Equal(HttpStatusCode.NotFound, result.HttpStatus);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Delete_Filter_And_Doesnt_Exist_Then_Not_Found_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _filterClient.Delete(baseUrl + "/filters", "filter_id", grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.True(result.ContainsError);
            Assert.Equal(HttpStatusCode.NotFound, result.HttpStatus);
        }

        #endregion

        #region Add

        [Fact]
        public async Task When_Add_Filter_And_No_Name_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _filterClient.Add(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.AddFilterRequest
            {
                Name = null
            }, grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter name is missing", result.Error.ErrorDescription);
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Update_Filter_And_No_Id_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _filterClient.Update(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.UpdateFilterRequest
            {
            }, grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter id is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Filter_And_No_Name_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _filterClient.Update(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.UpdateFilterRequest
            {
                Id = "invalid_filter"
            }, grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter name is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Filter_And_Filter_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            // ACT
            var result = await _filterClient.Update(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.UpdateFilterRequest
            {
                Id = "invalid_filter",
                Name = "name"
            }, grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.True(result.ContainsError);
            Assert.Equal(HttpStatusCode.NotFound, result.HttpStatus);
        }

        #endregion

        #endregion

        #region Happy paths

        #region Add + Get

        [Fact]
        public async Task When_Add_Filter_Then_Filter_Exists()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var addResult = await _filterClient.Add(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.AddFilterRequest
            {
                Name = "filter1",
                Rules = new List<AddFilterRuleRequest>
                {
                    new AddFilterRuleRequest
                    {
                        ClaimKey = "claim",
                        ClaimValue = "value",
                        Operation = AccountFilter.Basic.Common.ComparisonOperationsDto.RegularExpression
                    }
                }
            }, grantedToken.Content.AccessToken);

            // ACT
            var getResult = await _filterClient.Get(baseUrl + "/filters", addResult.Content.Id, grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.False(getResult.ContainsError);
            Assert.Equal("filter1", getResult.Content.Name);
            Assert.Equal(1, getResult.Content.Rules.Count());
            Assert.Equal("claim", getResult.Content.Rules.First().ClaimKey);
            Assert.Equal("value", getResult.Content.Rules.First().ClaimValue);
            Assert.Equal(AccountFilter.Basic.Common.ComparisonOperationsDto.RegularExpression, getResult.Content.Rules.First().Operation);
        }

        #endregion

        #region Add + Delete

        [Fact]
        public async Task When_Delete_Filter_Then_NoContent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var addResult = await _filterClient.Add(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.AddFilterRequest
            {
                Name = "filter1",
                Rules = new List<AddFilterRuleRequest>
                {
                    new AddFilterRuleRequest
                    {
                        ClaimKey = "claim",
                        ClaimValue = "value",
                        Operation = AccountFilter.Basic.Common.ComparisonOperationsDto.RegularExpression
                    }
                }
            }, grantedToken.Content.AccessToken);

            // ACT
            var deleteResult = await _filterClient.Delete(baseUrl + "/filters", addResult.Content.Id, grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.False(deleteResult.ContainsError);
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.HttpStatus);
        }

        #endregion

        #region Add + Get all

        [Fact]
        public async Task When_Get_All_Filters_Then_Several_Filters_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            _server.SharedCtx.Oauth2IntrospectionHttpClientFactory.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var grantedToken = await _clientAuthSelector.UseClientSecretPostAuth("stateless_client", "stateless_client")
                .UseClientCredentials("manage_account_filtering")
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var addResult = await _filterClient.Add(baseUrl + "/filters", new AccountFilter.Basic.Common.Requests.AddFilterRequest
            {
                Name = "filter1",
                Rules = new List<AddFilterRuleRequest>
                {
                    new AddFilterRuleRequest
                    {
                        ClaimKey = "claim",
                        ClaimValue = "value",
                        Operation = AccountFilter.Basic.Common.ComparisonOperationsDto.RegularExpression
                    }
                }
            }, grantedToken.Content.AccessToken);

            // ACT
            var getAllResults = await _filterClient.GetAll(baseUrl + "/filters", grantedToken.Content.AccessToken);

            // ASSERTS
            Assert.False(getAllResults.ContainsError);
            Assert.True(getAllResults.Content.Count() >= 1);
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var addFilterOperation = new AddFilterOperation(_httpClientFactoryStub.Object);
            var deleteFilterOperation = new DeleteFilterOperation(_httpClientFactoryStub.Object);
            var getAllFiltersOperation = new GetAllFiltersOperation(_httpClientFactoryStub.Object);
            var updateFilterOperation = new UpdateFilterOperation(_httpClientFactoryStub.Object);
            var getFilterOperation = new GetFilterOperation(_httpClientFactoryStub.Object);
            _filterClient = new FilterClient(
                addFilterOperation,
                deleteFilterOperation,
                getAllFiltersOperation,
                updateFilterOperation,
                getFilterOperation
            );
            var postTokenOperation = new PostTokenOperation(_httpClientFactoryStub.Object);
            var getDiscoveryOperation = new GetDiscoveryOperation(_httpClientFactoryStub.Object);
            var introspectionOperation = new IntrospectOperation(_httpClientFactoryStub.Object);
            var revokeTokenOperation = new RevokeTokenOperation(_httpClientFactoryStub.Object);
            _clientAuthSelector = new ClientAuthSelector(
                new TokenClientFactory(postTokenOperation, getDiscoveryOperation),
                new IntrospectClientFactory(introspectionOperation, getDiscoveryOperation),
                new RevokeTokenClientFactory(revokeTokenOperation, getDiscoveryOperation));
        }
    }
}
