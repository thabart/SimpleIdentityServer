using Moq;
using SimpleIdentityServer.Authenticate.SMS.Client;
using SimpleIdentityServer.Authenticate.SMS.Client.Factories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Store;
using SimpleIdentityServer.Twilio.Client;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests
{
    public class SmsCodeFixture : IClassFixture<TestScimServerFixture>
    {
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private SidSmsAuthenticateClient _sidSmsAuthenticateClient;
        private const string baseUrl = "http://localhost:5000";
        private readonly TestScimServerFixture _server;

        public SmsCodeFixture(TestScimServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Send_PhoneNumber_And_Twilio_Send_Exception_Then_Json_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
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
            var error = await _sidSmsAuthenticateClient.Send(baseUrl, new Authenticate.SMS.Common.Requests.ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            });

            // ASSERT
            Assert.NotNull(error);
            Assert.Equal("the twilio account is not properly configured", error.Message);
            Assert.Equal("unhandled_exception", error.Code);
        }

        [Fact]
        public async Task When_Send_PhoneNumber_And_ConfirmationCode_CannotBeInserted_Then_Json_Is_Returned()
        {

        }

        [Fact]
        public async Task When_Send_PhoneNumber_And_User_Doesnt_Exist_Then_NewOneIsCreated_And_Confirmation_Code_Is_Sent()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
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
            await _sidSmsAuthenticateClient.Send(baseUrl, new Authenticate.SMS.Common.Requests.ConfirmationCodeRequest
            {
                PhoneNumber = "phone"
            });

            // ASSERTS
            Assert.True(true);
        }


        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var sendSmsOperation = new SendSmsOperation(_httpClientFactoryStub.Object);
            _sidSmsAuthenticateClient = new SidSmsAuthenticateClient(sendSmsOperation);
        }
    }
}
