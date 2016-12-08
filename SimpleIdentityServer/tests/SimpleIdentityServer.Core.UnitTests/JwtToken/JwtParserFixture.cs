using Moq;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.UnitTests.Fake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    public sealed class JwtParserFixture
    {
        private Mock<IJweParser> _jweParserMock;
        private Mock<IJwsParser> _jwsParserMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IClientRepository> _clientRepositoryStub;
        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterMock;
        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryMock;
        private IJwtParser _jwtParser;

        #region DecryptAsync method

        [Fact]
        public async Task When_Passing_Empty_String_To_DecryptAsync_Without_ClientId_Method_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtParser.DecryptAsync(string.Empty));
        }

        [Fact]
        public async Task When_Passing_Not_Valid_Header_To_DecryptAsync_Without_ClientId_Method_Then_Empty_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = await _jwtParser.DecryptAsync("jws");

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public async Task When_Passing_Jwe_With_Not_Existing_JsonWebKey_To_DecryptAsync_Without_ClientId_Method_Then_Empty_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKidAsync(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = await _jwtParser.DecryptAsync("jws");

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_To_DecryptAsync_Without_ClientId_Method_Then_Parse_Method_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var jsonWebKey = new JsonWebKey();
            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(jsonWebKey));

            // ACT
            var result = await _jwtParser.DecryptAsync("jws");

            // ASSERT
            _jweParserMock.Verify(j => j.Parse(It.IsAny<string>(), It.IsAny<JsonWebKey>()));
        }

        #endregion

        #region DecryptAsync with client_id method

        [Fact]
        public async Task When_Passing_Empty_String_To_DecryptAsync_Method_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<AggregateException>(() => _jwtParser.DecryptAsync(string.Empty, string.Empty));
            await Assert.ThrowsAsync<AggregateException>(() => _jwtParser.DecryptAsync("jwe", string.Empty));
        }

        [Fact]
        public async Task When_Passing_Invalid_Client_To_Function_DecryptAsync_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Models.Client)null));

            // ACT & ASSERT
            var aggr = await Assert.ThrowsAsync<AggregateException>(() => _jwtParser.DecryptAsync("jws", clientId));
            var ex = aggr.InnerExceptions.First();
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
        }

        [Fact]
        public async Task When_Passing_Jws_With_Invalid_Header_To_DecryptAsync_Then_Empty_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Models.Client();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            var result = await _jwtParser.DecryptAsync("jws", "client_id");

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public async Task When_Passing_Jws_To_DecryptAsync_And_Cannot_Extract_Json_Web_Key_Then_Empty_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>()
            };
            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            var result = await _jwtParser.DecryptAsync("jws", clientId);

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public async Task When_Passing_Jws_With_Valid_Kid_To_DecryptAsync_Then_Parse_Is_Called_And_Payload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            const string kid = "1";
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid,
                SerializedKey = "serialized_key"
            };
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JweAlgNames.A128KW,
                Kid = kid
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>
                {
                    jsonWebKey
                }
            };

            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            await _jwtParser.DecryptAsync(jws, clientId);

            // ASSERT
            _jweParserMock.Verify(j => j.Parse(jws, jsonWebKey));
        }

        #endregion

        #region DecryptAsync with password method
        
        [Fact]
        public async Task When_Passing_Jws_To_DecryptAsync_With_Password_And_Cannot_Extract_Json_Web_Key_Then_Empty_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string password = "password";
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>()
            };
            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            var result = await _jwtParser.DecryptWithPasswordAsync("jws", clientId, password);

            // ASSERT
            Assert.Empty(result);
        }


        [Fact]
        public async Task When_DecryptAsync_Jwe_With_Password_Then_Function_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            const string kid = "1";
            const string password = "password";
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid,
                SerializedKey = "serialized_key"
            };
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JweAlgNames.A128KW,
                Kid = kid
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>
                {
                    jsonWebKey
                }
            };

            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            await _jwtParser.DecryptWithPasswordAsync(jws, clientId, password);

            // ASSERT
            _jweParserMock.Verify(j => j.ParseByUsingSymmetricPassword(jws, jsonWebKey, password));
        }

        #endregion

        #region Unsign method

        [Fact]
        public async Task When_Passing_Null_To_Unsign_Without_ClientId_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwtParser.UnSignAsync(string.Empty));
        }

        [Fact]
        public async Task When_Passing_Not_Valid_Header_To_Unsign_Without_ClientId_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = await _jwtParser.UnSignAsync("jws");

            // ASSERT
            Assert.True(result == null);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_To_Unsign_Without_ClientId_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            const string kid = "1";
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid,
                SerializedKey = "serialized_key"
            };
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256,
                Kid = kid
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(jsonWebKey));
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            var result = await _jwtParser.UnSignAsync(jws);

            // ASSERT
            Assert.NotNull(result);
            _jwsParserMock.Verify(j => j.ValidateSignature(jws, jsonWebKey));
        }

        #endregion

        #region Unsign with client_id method

        [Fact]
        public async Task When_Passing_Empty_String_To_Function_Unsign_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<AggregateException>(() => _jwtParser.UnSignAsync(string.Empty, string.Empty));
            await Assert.ThrowsAsync<AggregateException>(() => _jwtParser.UnSignAsync("jws", string.Empty));
        }

        [Fact]
        public async Task When_Passing_Invalid_Client_To_Function_Unsign_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Models.Client)null));

            // ACT & ASSERTS
            var aggr = await Assert.ThrowsAsync<AggregateException>(() => _jwtParser.UnSignAsync("jws", clientId));
            Assert.NotNull(aggr);
            var ex = aggr.InnerExceptions.First();
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
        }

        [Fact]
        public async Task When_Passing_Jws_With_Invalid_Header_To_Function_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            const string clientId = "client_id";
            var client = new Models.Client();
            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = await _jwtParser.UnSignAsync("jws", clientId);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_Json_Web_Key_Uri_Is_Not_Valid_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Models.Client
            {
                JwksUri = "invalid_url",
                ClientId = clientId
            };
            var jsonWebKeys = new List<JsonWebKey>();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jsonWebKeyConverterMock.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = await _jwtParser.UnSignAsync("jws", clientId);

            // ASSERT
            Assert.Null(result);
        }
        
        [Fact]
        public async Task When_Requesting_Uri_Returned_Error_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string json = "json";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json)
            };
            var client = new Models.Client
            {
                JwksUri = "http://localhost",
                ClientId = clientId
            };
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryMock.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);

            // ACT
            var result = await _jwtParser.UnSignAsync("jws", clientId);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_No_Json_Web_Key_Can_Be_Extracted_From_Uri_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithDataContract();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var client = new Models.Client
            {
                JwksUri = "http://localhost",
                ClientId = clientId
            };
            var jsonWebKeys = new List<JsonWebKey>();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryMock.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterMock.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = await _jwtParser.UnSignAsync("jws", clientId);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_No_Json_Web_Key_Can_Be_Extracted_From_Jwks_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>()
            };
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            var result = await _jwtParser.UnSignAsync("jws", clientId);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_No_Uri_And_JsonWebKeys_Are_Defined_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Models.Client
            {
                ClientId = clientId
            };
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            // ACT
            var result = await _jwtParser.UnSignAsync("jws", clientId);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_Passing_Jws_With_None_Algorithm_To_Unsign_And_No_Uri_And_Jwks_Are_Defined_Then_Payload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.NONE
            };
            var client = new Models.Client
            {
                ClientId = clientId
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwsParserMock.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(payLoad);

            // ACT
            var result = await _jwtParser.UnSignAsync(jws, clientId);

            // ASSERT
            Assert.NotNull(result);
            _jwsParserMock.Verify(j => j.GetPayload(jws));
        }

        [Fact]
        public async Task When_Passing_Jws_With_Algorithm_Other_Than_None_To_Unsign_And_Retrieve_Json_Web_Key_From_Uri_Then_Jwis_Is_Unsigned_And_Payload_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            const string kid = "1";
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithDataContract();
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid,
                SerializedKey = "serialized_key"
            };
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256,
                Kid = kid
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JwksUri = "http://localhost"
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var jsonWebKeys = new List<JsonWebKey>
            {
                jsonWebKey
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);
            _httpClientFactoryMock.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterMock.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = await _jwtParser.UnSignAsync(jws, clientId);

            // ASSERT
            Assert.NotNull(result);
            _jwsParserMock.Verify(j => j.ValidateSignature(jws, jsonWebKey));
        }
        
        [Fact]
        public async Task When_Passing_Jws_With_Algorithm_Other_Than_None_To_Unsign_And_Retrieve_Json_Web_Key_From_Parameter_Then_Jws_Is_Unsigned_And_Payload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            const string kid = "1";
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid,
                SerializedKey = "serialized_key"
            };
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256,
                Kid = kid
            };
            var client = new Models.Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>
                {
                    jsonWebKey
                }
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientRepositoryStub.Setup(c => c.GetClientByIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            var result = await _jwtParser.UnSignAsync(jws, clientId);

            // ASSERT
            Assert.NotNull(result);
            _jwsParserMock.Verify(j => j.ValidateSignature(jws, jsonWebKey));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jweParserMock = new Mock<IJweParser>();
            _jwsParserMock = new Mock<IJwsParser>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _clientRepositoryStub = new Mock<IClientRepository>();
            _jsonWebKeyConverterMock = new Mock<IJsonWebKeyConverter>();
            _jsonWebKeyRepositoryMock = new Mock<IJsonWebKeyRepository>();
            _jwtParser = new JwtParser(
                _jweParserMock.Object,
                _jwsParserMock.Object,
                _httpClientFactoryMock.Object,
                _clientRepositoryStub.Object,
                _jsonWebKeyConverterMock.Object,
                _jsonWebKeyRepositoryMock.Object);
        }
    }
}
