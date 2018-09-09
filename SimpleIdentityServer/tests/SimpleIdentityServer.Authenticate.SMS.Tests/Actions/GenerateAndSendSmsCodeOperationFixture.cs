using Moq;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.OpenId.Logging;
using SimpleIdentityServer.Store;
using SimpleIdentityServer.Twilio.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Authenticate.SMS.Tests.Actions
{
    public class GenerateAndSendSmsCodeOperationFixture
    {
        private const string _message = "Message {0}";
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private SmsAuthenticationOptions _smsAuthenticationOptions;
        private Mock<ITwilioClient> _twilioClientStub;
        private Mock<IOpenIdEventSource> _eventSourceStub;
        private IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

        [Fact]
        public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAndSendSmsCodeOperation.Execute(null)).ConfigureAwait(false);
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_TwilioSendException_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _twilioClientStub.Setup(s => s.SendMessage(It.IsAny<TwilioSmsCredentials>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new TwilioException("problem");
                });

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendSmsCodeOperation.Execute("phoneNumber")).ConfigureAwait(false);

            // ACT
            _eventSourceStub.Verify(e => e.Failure(It.Is<Exception>((f) => f.Message == "problem")));
            Assert.NotNull(exception);
            Assert.Equal("unhandled_exception", exception.Code);
            Assert.Equal("the twilio account is not properly configured", exception.Message);
        }

        [Fact]
        public async Task When_CannotInsert_ConfirmationCode_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>())).Returns(() => Task.FromResult(false));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _generateAndSendSmsCodeOperation.Execute("phoneNumber")).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal("unhandled_exception", exception.Code);
            Assert.Equal("the confirmation code cannot be saved", exception.Message);
        }

        [Fact]
        public async Task When_GenerateAndSendConfirmationCode_Then_Code_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>())).Returns(() => Task.FromResult(true));

            // ACT
            var confirmationCode = await _generateAndSendSmsCodeOperation.Execute("phoneNumber").ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(confirmationCode);
            _eventSourceStub.Verify(e => e.GetConfirmationCode(It.IsAny<string>()));
        }

        private void InitializeFakeObjects()
        {
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _smsAuthenticationOptions = new SmsAuthenticationOptions
            {
                Message = _message
            };
            _twilioClientStub = new Mock<ITwilioClient>();
            _eventSourceStub = new Mock<IOpenIdEventSource>();
            _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(_confirmationCodeStoreStub.Object,
                _smsAuthenticationOptions,
                _twilioClientStub.Object,
                _eventSourceStub.Object);
        }
    }
}
