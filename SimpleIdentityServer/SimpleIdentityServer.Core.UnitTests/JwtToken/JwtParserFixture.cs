using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;

using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    [TestFixture]
    public sealed class JwtParserFixture
    {
        private Mock<IJweParser> _jweParserMock;

        private Mock<IJwsParser> _jwsParserMock;

        private Mock<IHttpClientFactory> _httpClientFactoryMock;

        private Mock<IClientValidator> _clientValidatorMock;

        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterMock;

        private IJwtParser _jwtParser;

        #region Decrypt method

        [Test]
        public void When_Passing_Empty_String_To_Decrypt_Method_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtParser.Decrypt(string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => _jwtParser.Decrypt("jwe", string.Empty));
        }

        [Test]
        public void When_Passing_Invalid_Client_To_Function_Decrypt_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERT
            var ex = Assert.Throws<InvalidOperationException>(() => _jwtParser.Decrypt("jws", clientId));
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
        }

        [Test]
        public void When_Passing_Jws_With_Invalid_Header_To_Decrypt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var client = new Client();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _jwtParser.Decrypt("jws", "client_id");

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_To_Decrypt_And_Cannot_Extract_Json_Web_Key_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JweProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>()
            };
            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _jwtParser.Decrypt("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_With_Valid_Kid_To_Decrypt_Then_Parse_Is_Called_And_Payload_Is_Returned()
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
            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>
                {
                    jsonWebKey
                }
            };

            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            _jwtParser.Decrypt(jws, clientId);

            // ASSERT
            _jweParserMock.Verify(j => j.Parse(jws, jsonWebKey));
        }

        #endregion

        #region Unsign method

        [Test]
        public void When_Passing_Empty_String_To_Function_Unsign_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtParser.UnSign(string.Empty, string.Empty));
            Assert.Throws<ArgumentNullException>(() => _jwtParser.UnSign("jws", string.Empty));
        }

        [Test]
        public void When_Passing_Invalid_Client_To_Function_Unsign_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string clientId = "client_id";
            InitializeFakeObjects();
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jwtParser.UnSign("jws", clientId));
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
        }

        [Test]
        public void When_Passing_Jws_With_Invalid_Header_To_Function_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            const string clientId = "client_id";
            var client = new Client();
            InitializeFakeObjects();
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _jwtParser.UnSign("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Json_Web_Key_Uri_Is_Not_Valid_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Client
            {
                JwksUri = "invalid_url",
                ClientId = clientId
            };
            var jsonWebKeys = new List<JsonWebKey>();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _jsonWebKeyConverterMock.Setup(j => j.ConvertFromJson(It.IsAny<string>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = _jwtParser.UnSign("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }
        
        [Test]
        public void When_Requesting_Uri_Returned_Error_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
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
            var client = new Client
            {
                JwksUri = "http://localhost",
                ClientId = clientId
            };
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryMock.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);

            // ACT
            var result = _jwtParser.UnSign("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_No_Json_Web_Key_Can_Be_Extracted_From_Uri_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string json = "json";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var client = new Client
            {
                JwksUri = "http://localhost",
                ClientId = clientId
            };
            var jsonWebKeys = new List<JsonWebKey>();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryMock.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterMock.Setup(j => j.ConvertFromJson(It.IsAny<string>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = _jwtParser.UnSign("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_No_Json_Web_Key_Can_Be_Extracted_From_Jwks_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>()
            };
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _jwtParser.UnSign("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_No_Uri_And_JsonWebKeys_Are_Defined_And_Algorithm_Is_PS256_And_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "client_id";
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var client = new Client
            {
                ClientId = clientId
            };
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);

            // ACT
            var result = _jwtParser.UnSign("jws", clientId);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_With_None_Algorithm_To_Unsign_And_No_Uri_And_Jwks_Are_Defined_Then_Payload_Is_Returned()
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
            var client = new Client
            {
                ClientId = clientId
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _jwsParserMock.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(payLoad);

            // ACT
            var result = _jwtParser.UnSign(jws, clientId);

            // ASSERT
            Assert.IsNotNull(result);
            _jwsParserMock.Verify(j => j.GetPayload(jws));
        }

        [Test]
        public void When_Passing_Jws_With_Algorithm_Other_Than_None_To_Unsign_And_Retrieve_Json_Web_Key_From_Uri_Then_Jwis_Is_Unsigned_And_Payload_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            const string clientId = "client_id";
            const string kid = "1";
            const string json = "json";
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
            var client = new Client
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
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);
            _httpClientFactoryMock.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterMock.Setup(j => j.ConvertFromJson(It.IsAny<string>()))
                .Returns(jsonWebKeys);

            // ACT
            var result = _jwtParser.UnSign(jws, clientId);

            // ASSERT
            Assert.IsNotNull(result);
            _jwsParserMock.Verify(j => j.ValidateSignature(jws, jsonWebKey));
        }
        
        [Test]
        public void When_Passing_Jws_With_Algorithm_Other_Than_None_To_Unsign_And_Retrieve_Json_Web_Key_From_Parameter_Then_Jws_Is_Unsigned_And_Payload_Is_Returned()
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
            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys = new List<JsonWebKey>
                {
                    jsonWebKey
                }
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _clientValidatorMock.Setup(c => c.ValidateClientExist(It.IsAny<string>()))
                .Returns(client);
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            var result = _jwtParser.UnSign(jws, clientId);

            // ASSERT
            Assert.IsNotNull(result);
            _jwsParserMock.Verify(j => j.ValidateSignature(jws, jsonWebKey));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jweParserMock = new Mock<IJweParser>();
            _jwsParserMock = new Mock<IJwsParser>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _clientValidatorMock = new Mock<IClientValidator>();
            _jsonWebKeyConverterMock = new Mock<IJsonWebKeyConverter>();
            _jwtParser = new JwtParser(
                _jweParserMock.Object,
                _jwsParserMock.Object,
                _httpClientFactoryMock.Object,
                _clientValidatorMock.Object,
                _jsonWebKeyConverterMock.Object);
        }
    }
}
