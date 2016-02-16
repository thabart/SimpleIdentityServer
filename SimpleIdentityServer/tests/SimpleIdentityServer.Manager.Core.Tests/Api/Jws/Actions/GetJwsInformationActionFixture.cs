using Moq;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Api.Jws.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws.Actions
{
    public class GetJwsInformationActionFixture
    {
        private Mock<IJwsParser> _jwsParserStub;

        private IGetJwsInformationAction _getJwsInformationAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _getJwsInformationAction.Execute(null));
            Assert.Throws<ArgumentNullException>(() => _getJwsInformationAction.Execute(getJwsParameter));
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
            var exception = Assert.Throws<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
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
            var exception = Assert.Throws<IdentityServerManagerException>(() => _getJwsInformationAction.Execute(getJwsParameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenIsNotAValidJws);
        }

        private void InitializeFakeObjects()
        {
            _jwsParserStub = new Mock<IJwsParser>();
            _getJwsInformationAction = new GetJwsInformationAction(_jwsParserStub.Object);
        }
    }
}
