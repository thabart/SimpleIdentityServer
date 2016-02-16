using Moq;
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

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            var firstException = Assert.Throws<AggregateException>(() => _getJwsInformationAction.Execute(null).Result);
            var secondException = Assert.Throws<AggregateException>(() => _getJwsInformationAction.Execute(getJwsParameter).Result);
            Assert.True(firstException.InnerExceptions.First() is ArgumentNullException);
            Assert.True(secondException.InnerExceptions.First() is ArgumentNullException);
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
            var exception = Assert.Throws<AggregateException>(() => _getJwsInformationAction.Execute(getJwsParameter).Result);
            var innerException = exception.InnerExceptions.First() as IdentityServerManagerException;
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
            var exception = Assert.Throws<AggregateException>(() => _getJwsInformationAction.Execute(getJwsParameter).Result);
            var innerException = exception.InnerExceptions.First() as IdentityServerManagerException;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheTokenIsNotAValidJws);
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
                Kid = kid
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
            var exception = Assert.Throws<AggregateException>(() => _getJwsInformationAction.Execute(getJwsParameter).Result);
            var innerException = exception.InnerExceptions.First() as IdentityServerManagerException;
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

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
