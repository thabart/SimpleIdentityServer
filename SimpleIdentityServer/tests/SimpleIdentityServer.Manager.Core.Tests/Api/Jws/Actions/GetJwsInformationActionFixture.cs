using Moq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Api.Jws.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Factories;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Tests.Fake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws.Actions
{
    public class GetJwsInformationActionFixture
    {
        private Mock<IJwsParser> _jwsParserStub;

        private Mock<IHttpClientFactory> _httpClientFactoryStub;

        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterStub;

        private IGetJwsInformationAction _getJwsInformationAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<AggregateException>(() => _getJwsInformationAction.Execute(null));
            Assert.ThrowsAsync<AggregateException>(() => _getJwsInformationAction.Execute(getJwsParameter));
        }

        [Fact]
        public void When_Passing_Not_Well_Formed_Url_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "not_well_formed";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };

            // ACTS & ASSERTS
            var exception = Assert.ThrowsAsync<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            var innerException = exception.Result;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        }

        [Fact]
        public void When_Passing_A_Not_Valid_Jws_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Url = "http://google.be",
                Jws = "jws"
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            var innerException = exception.Result;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheTokenIsNotAValidJws);
        }

        [Fact]
        public void When_No_Uri_And_Sign_Alg_Are_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = Constants.JwsAlgNames.RS256
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            var innerException = exception.Result;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureCannotBeChecked);
        }

        [Fact]
        public void When_JsonWebKey_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid,
                Alg = Constants.JwsAlgNames.RS256
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("")
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            var innerException = exception.Result;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public void When_JsonWebKey_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid,
                Alg = Constants.JwsAlgNames.RS256
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(() => new List<JsonWebKey>());

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            var innerException = exception.Result;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public void When_The_Signature_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(() => new List<JsonWebKey>
                {
                    new JsonWebKey
                    {
                        Kid = kid
                    }
                });
            _jwsParserStub.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.ThrowsAsync<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            var innerException = exception.Result;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_JsonWebKey_Is_Extracted_And_The_Jws_Is_Unsigned_Then_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var jwsPayload = new JwsPayload();
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _httpClientFactoryStub.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);
            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(() => new List<JsonWebKey>
                {
                    new JsonWebKey
                    {
                        Kid = kid
                    }
                });
            _jwsParserStub.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);

            // ACT
            var result = _getJwsInformationAction.Execute(getJwsParameter).Result;

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.JsonWebKey.Kid == kid);
        }

        [Fact]
        public void When_Extracting_Information_Of_Unsigned_Jws_Then_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = Constants.JwsAlgNames.NONE
            };
            var jwsPayload = new JwsPayload();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserStub.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);

            // ACT
            var result = _getJwsInformationAction.Execute(getJwsParameter).Result;

            // ASSERTS
            Assert.NotNull(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jwsParserStub = new Mock<IJwsParser>();
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            _jsonWebKeyConverterStub = new Mock<IJsonWebKeyConverter>();
            _getJwsInformationAction = new GetJwsInformationAction(
                _jwsParserStub.Object, 
                _httpClientFactoryStub.Object,
                _jsonWebKeyConverterStub.Object);
        }
    }
}
