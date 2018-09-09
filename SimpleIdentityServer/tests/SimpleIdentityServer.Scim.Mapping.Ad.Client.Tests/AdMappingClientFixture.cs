using Moq;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests
{
    using Common.DTOs.Requests;

    public class AdMappingClientFixture : IClassFixture<TestAdMappingServerFixture>
    {
        private readonly TestAdMappingServerFixture _testAdMappingServerFixture;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IAdMappingClient _adMappingClient;

        public AdMappingClientFixture(TestAdMappingServerFixture testAdMappingServerFixture)
        {
            _testAdMappingServerFixture = testAdMappingServerFixture;
        }

        #region Errors

        #region Add mapping

        [Fact]
        public async Task When_Add_Mapping_And_No_AttributeId_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.AddMapping(new AddMappingRequest(), "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter attribute_id is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Mapping_And_No_AdPropertyName_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.AddMapping(new AddMappingRequest
            {
                AttributeId = "att"
            }, "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter ad_property_name is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Mapping_And_No_SchemaId_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.AddMapping(new AddMappingRequest
            {
                AttributeId = "att",
                AdPropertyName = "adpropertyname"
            }, "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter schema_id is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Mapping_And_There_Is_Already_One_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.AddMapping(new AddMappingRequest
            {
                AttributeId = "attributeid",
                SchemaId = "schema",
                AdPropertyName = "prop"
            }, "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("a mapping has already been assigned", result.Error.ErrorDescription);
        }

        #endregion

        #region Remove mapping

        [Fact]
        public async Task When_Remove_Mapping_And_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.DeleteMapping("invalidattributeid", "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the mapping doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #region Get mapping

        [Fact]
        public async Task When_Get_Mapping_And_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.GetAdMapping("invalidattributeid", "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the mapping doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #endregion

        #region Happy path

        #region Add mapping

        [Fact]
        public async Task When_Add_Mapping_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.AddMapping(new AddMappingRequest
            {
                AttributeId = "newattribute",
                AdPropertyName = "prop",
                SchemaId = "schemaid",
            }, "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        #endregion

        #region Remove mapping

        [Fact]
        public async Task When_Remove_Mapping_Then_Ok_Is_Returned()
        {
            // ARRANGE
            var attrId = Guid.NewGuid().ToString();
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);
            var result = await _adMappingClient.AddMapping(new AddMappingRequest
            {
                AttributeId = attrId,
                AdPropertyName = "prop",
                SchemaId = "schemaid"
            }, "http://localhost:5000", null).ConfigureAwait(false);

            // ACT
            await _adMappingClient.DeleteMapping(attrId, "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        #endregion

        #region Get mapping

        [Fact]
        public async Task When_Get_Mapping_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.GetAdMapping("attributeid", "http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        #endregion

        #region Get all mappings

        [Fact]
        public async Task When_Get_All_Mappings_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_testAdMappingServerFixture.Client);

            // ACT
            var result = await _adMappingClient.GetAllMappings("http://localhost:5000", null).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var addAdMappingOperation = new AddAdMappingOperation(_httpClientFactoryStub.Object);
            var deleteMappingOperation = new DeleteAdMappingOperation(_httpClientFactoryStub.Object);
            var getAdMappingOperation = new GetAdMappingOperation(_httpClientFactoryStub.Object);
            var getAllAdMappingsOperation = new GetAllAdMappingsOperation(_httpClientFactoryStub.Object);
            var getAllPropertiesOperation = new GetAdPropertiesOperation(_httpClientFactoryStub.Object);
            _adMappingClient = new AdMappingClient(addAdMappingOperation, deleteMappingOperation,
                getAdMappingOperation, getAllAdMappingsOperation, getAllPropertiesOperation);
        }
    }
}
