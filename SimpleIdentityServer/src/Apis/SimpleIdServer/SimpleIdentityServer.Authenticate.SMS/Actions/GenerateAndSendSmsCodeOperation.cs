using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Store;
using SimpleIdentityServer.Twilio.Client;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Actions
{
    public interface IGenerateAndSendSmsCodeOperation
    {
        Task<string> Execute(string phoneNumber);
    }

    internal sealed class GenerateAndSendSmsCodeOperation : IGenerateAndSendSmsCodeOperation
    {
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly SmsAuthenticationOptions _smsAuthenticationOptions;
        private readonly ITwilioClient _twilioClient;

        public GenerateAndSendSmsCodeOperation(IConfirmationCodeStore confirmationCodeStore, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _smsAuthenticationOptions = smsAuthenticationOptions;
            _twilioClient = new TwilioClient();
        }

        public async Task<string> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode(),
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 300
            };

            if (!await _confirmationCodeStore.Add(confirmationCode))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
            }

            var message = string.Format(_smsAuthenticationOptions.Message, confirmationCode.Value);
            await _twilioClient.SendMessage(_smsAuthenticationOptions.TwilioSmsCredentials, phoneNumber, message);
            return confirmationCode.Value;
        }

        private async Task<string> GetCode()
        {
            var random = new Random();
            var number = random.Next(100000, 999999);
            if (await _confirmationCodeStore.Get(number.ToString()) != null)
            {
                return await GetCode();
            }

            return number.ToString();
        }
    }
}
