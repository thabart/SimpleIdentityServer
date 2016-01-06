using System;

using Moq;

using NUnit.Framework;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    [TestFixture]
    public sealed class JwtParserFixture
    {
        private Mock<IJweParser> _jweParserMock;

        private Mock<IJwsParser> _jwsParserMock;

        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryMock;

        private IJwtParser _jwtParser;

        #region Decrypt method

        [Test]
        public void When_Passing_Empty_String_To_Decrypt_Method_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtParser.Decrypt(string.Empty));
        }

        [Test]
        public void When_Passing_Jws_With_Invalid_Header_To_Decrypt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _jwtParser.Decrypt("jws");

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_With_Not_Valid_Kid_To_Decrypt_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(new JwsProtectedHeader());
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKid(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _jwtParser.Decrypt("jws");

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_With_Valid_Kid_To_Decrypt_Then_Parse_Is_Called_And_Payload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jwe = "jwe";
            var jwsProtectedHeader = new JweProtectedHeader();
            var jsonWebKey = new JsonWebKey();
            _jweParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKid(It.IsAny<string>()))
                .Returns(jsonWebKey);
            _jweParserMock.Setup(j => j.Parse(It.IsAny<string>(),
                It.IsAny<JsonWebKey>()))
                .Returns(string.Empty);

            // ACT
            var result = _jwtParser.Decrypt(jwe);

            // ASSERT
            Assert.IsNotNull(result);
            _jweParserMock.Verify(j => j.Parse(jwe, jsonWebKey));
        }

        #endregion

        #region Unsign method

        [Test]
        public void When_Passing_Empty_String_To_Function_Unsign_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jwtParser.UnSign(string.Empty));
        }

        [Test]
        public void When_Passing_Jws_With_Invalid_Header_To_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _jwtParser.UnSign("jws");
            
            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_With_No_Kid_And_An_Algorithm_Different_From_None_To_Unsign_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKid(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _jwtParser.UnSign("jws");

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Jws_With_None_Algorithm_To_Unsign_Then_Payload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.NONE
            };

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKid(It.IsAny<string>()))
                .Returns(() => null);
            _jwsParserMock.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(payLoad);

            // ACT
            var result = _jwtParser.UnSign(jws);

            // ASSERT
            Assert.IsNotNull(result);
            _jwsParserMock.Verify(j => j.GetPayload(jws));
        }

        [Test]
        public void When_Passing_Jws_With_Algorithm_Other_Than_None_To_Unsign_Then_Jws_Is_Unsigned_And_Payload_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string jws = "jws";
            var payLoad = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Jwt.Constants.JwsAlgNames.PS256
            };
            var jsonWebKey = new JsonWebKey();

            _jwsParserMock.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyRepositoryMock.Setup(j => j.GetByKid(It.IsAny<string>()))
                .Returns(jsonWebKey);
            _jwsParserMock.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(payLoad);

            // ACT
            var result = _jwtParser.UnSign(jws);

            // ASSERT
            Assert.IsNotNull(result);
            _jwsParserMock.Verify(j => j.ValidateSignature(jws, jsonWebKey));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jweParserMock = new Mock<IJweParser>();
            _jwsParserMock = new Mock<IJwsParser>();
            _jsonWebKeyRepositoryMock = new Mock<IJsonWebKeyRepository>();
            _jwtParser = new JwtParser(
                _jweParserMock.Object,
                _jwsParserMock.Object,
                _jsonWebKeyRepositoryMock.Object);
        }
    }
}
