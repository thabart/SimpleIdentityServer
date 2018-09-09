using Moq;
using SimpleIdentityServer.Authenticate.SMS.Client;
using SimpleIdentityServer.Authenticate.SMS.Client.Factories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Store;
using SimpleIdentityServer.Twilio.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    using Authenticate.SMS.Common.Requests;

    public class SmsCodeFixture : IClassFixture<TestOauthServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private SidSmsAuthenticateClient _sidSmsAuthenticateClient;
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public SmsCodeFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Send_ConfirmationCode_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT : NO PHONE NUMBER
            var noPhoneNumberResult = await _sidSmsAuthenticateClient.Send(baseUrl, new ConfirmationCodeRequest
            {
                PhoneNumber = string.Empty
            }).ConfigureAwait(false);
            // ACT : TWILIO NO CONFIGURED
            ConfirmationCode confirmationCode = new ConfirmationCode();
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult((ConfirmationCode)null);
            });
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>())).Callback<ConfirmationCode>(r =>
            {
                confirmationCode = r;
            }).Returns(() =>
            {
                return Task.FromResult(true);
            });
            _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<TwilioSmsCredentials>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, "the twilio account is not properly configured");
                });
            var twilioNotConfigured = await _sidSmsAuthenticateClient.Send(baseUrl, new ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            }).ConfigureAwait(false);
            // ACT : NO CONFIRMATION CODE
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult((ConfirmationCode)null);
            });
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>())).Callback<ConfirmationCode>(r =>
            {
                confirmationCode = r;
            }).Returns(() =>
            {
                return Task.FromResult(false);
            });
            _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<TwilioSmsCredentials>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { }).Returns(Task.FromResult(true));
            var cannotInsertConfirmationCode = await _sidSmsAuthenticateClient.Send(baseUrl, new ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            }).ConfigureAwait(false);
            // ACT : UNHANDLED EXCEPTION
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult((ConfirmationCode)null);
            });
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>())).Callback(() =>
            {
                throw new Exception();
            }).Returns(() =>
            {
                return Task.FromResult(false);
            });
            var unhandledException = await _sidSmsAuthenticateClient.Send(baseUrl, new ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            }).ConfigureAwait(false);
            // ACT : HAPPY PATH
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult((ConfirmationCode)null);
            });
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>())).Callback<ConfirmationCode>(r =>
            {
                confirmationCode = r;
            }).Returns(() =>
            {
                return Task.FromResult(true);
            });
            _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<TwilioSmsCredentials>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { }).Returns(Task.FromResult(true));
            var happyPath = await _sidSmsAuthenticateClient.Send(baseUrl, new ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            }).ConfigureAwait(false);


            // ASSERT : NO PHONE NUMBER
            Assert.NotNull(noPhoneNumberResult);
            Assert.True(noPhoneNumberResult.ContainsError);
            Assert.Equal(HttpStatusCode.BadRequest, noPhoneNumberResult.HttpStatus);
            Assert.Equal("invalid_request", noPhoneNumberResult.Error.Error);
            Assert.Equal("parameter phone_number is missing", noPhoneNumberResult.Error.ErrorDescription);
            // ASSERT : TWILIO NOT CONFIGURED
            Assert.NotNull(twilioNotConfigured);
            Assert.True(twilioNotConfigured.ContainsError);
            Assert.Equal("unhandled_exception", twilioNotConfigured.Error.Error);
            Assert.Equal("the twilio account is not properly configured", twilioNotConfigured.Error.ErrorDescription);
            Assert.Equal(HttpStatusCode.InternalServerError, twilioNotConfigured.HttpStatus);            
            // ASSERT : CANNOT INSERT CONFIRMATION CODE
            Assert.NotNull(cannotInsertConfirmationCode);
            Assert.True(cannotInsertConfirmationCode.ContainsError);
            Assert.Equal("unhandled_exception", cannotInsertConfirmationCode.Error.Error);
            Assert.Equal("the confirmation code cannot be saved", cannotInsertConfirmationCode.Error.ErrorDescription);
            Assert.Equal(HttpStatusCode.InternalServerError, cannotInsertConfirmationCode.HttpStatus);
            // ASSERT : UNHANDLED EXCEPTION
            Assert.NotNull(unhandledException);
            Assert.True(unhandledException.ContainsError);
            Assert.Equal("unhandled_exception", unhandledException.Error.Error);
            Assert.Equal("unhandled exception occured please contact the administrator", unhandledException.Error.ErrorDescription);
            Assert.Equal(HttpStatusCode.InternalServerError, unhandledException.HttpStatus);
            // ASSERT : HAPPY PATH
            Assert.True(true);
            Assert.NotNull(happyPath);
            Assert.False(happyPath.ContainsError);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var sendSmsOperation = new SendSmsOperation(_httpClientFactoryStub.Object);
            _sidSmsAuthenticateClient = new SidSmsAuthenticateClient(sendSmsOperation);
        }
    }
}
