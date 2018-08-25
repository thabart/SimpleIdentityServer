using Moq;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests
{
    public class AdConfigurationClientFixture : IClassFixture<TestAdMappingServerFixture>
    {
        private readonly TestAdMappingServerFixture _testAdMappingServerFixture;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IAdConfigurationClient _adConfigurationClient;

        public AdConfigurationClientFixture(TestAdMappingServerFixture testAdMappingServerFixture)
        {
            _testAdMappingServerFixture = testAdMappingServerFixture;
        }

        #region Errors

        #region Update configuration

        [Fact]
        public async Task When_Update_Configuration_And_No_IpAdr_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter ip_adr is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_IpAdr_Is_Not_Valid_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "invalidip" }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("not valid ip address", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_Port_Is_Not_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "127.0.0.1" }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter port is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_UserName_Is_Not_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "127.0.0.1", Port = 1000 }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter username is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_Password_Is_Not_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "127.0.0.1", Port = 1000, Username = "username" }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter password is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_DistinguishedName_Is_Not_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "127.0.0.1", Port = 1000, Username = "username", Password = "pass" }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter distinguished_name is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_UserFilter_Is_Not_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "127.0.0.1", Port = 1000, Username = "username", Password = "pass", DistinguishedName = "dn" }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter user_filter is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Configuration_And_UserFilterClass_Is_Not_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = true, IpAdr = "127.0.0.1", Port = 1000, Username = "username", Password = "pass", DistinguishedName = "dn", UserFilter = "userfilter" }, "http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter user_filter_class is missing", result.Error.ErrorDescription);
        }

        #endregion

        #endregion

        #region Happy path

        [Fact]
        public async Task When_Update_Configuration_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);
            var result = await _adConfigurationClient.UpdateConfiguration(new Common.DTOs.Requests.UpdateAdConfigurationRequest { IsEnabled = false, IpAdr = "127.0.0.1", Port = 1000, Username = "username", Password = "pass", DistinguishedName = "dn" }, "http://localhost:5000", null);

            // ACT
            var getResult = await _adConfigurationClient.GetConfiguration("http://localhost:5000", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.False(getResult.ContainsError);
            Assert.Equal("127.0.0.1", getResult.Content.IpAdr);
            Assert.Equal(1000, getResult.Content.Port);
            Assert.Equal("username", getResult.Content.Username);
            Assert.Equal("pass", getResult.Content.Password);
            Assert.Equal("dn", getResult.Content.DistinguishedName);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var getAdConfigurationOperation = new GetAdConfigurationOperation(_httpClientFactoryStub.Object);
            var updateAdConfigurationOperation = new UpdateAdConfigurationOperation(_httpClientFactoryStub.Object);
            _adConfigurationClient = new AdConfigurationClient(getAdConfigurationOperation, updateAdConfigurationOperation);
        }
    }
}
