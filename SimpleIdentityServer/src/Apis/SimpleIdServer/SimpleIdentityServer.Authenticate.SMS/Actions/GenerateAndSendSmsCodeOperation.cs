using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.OpenId.Logging;
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
        private readonly IOpenIdEventSource _eventSource;

        public GenerateAndSendSmsCodeOperation(IConfirmationCodeStore confirmationCodeStore, SmsAuthenticationOptions smsAuthenticationOptions,
            ITwilioClient twilioClient, IOpenIdEventSource eventSource)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _smsAuthenticationOptions = smsAuthenticationOptions;
            _twilioClient = twilioClient;
            _eventSource = eventSource;
        }

        public async Task<string> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode().ConfigureAwait(false),
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 300,
                Subject = phoneNumber
            };

            var message = string.Format(_smsAuthenticationOptions.Message, confirmationCode.Value);
            try
            {
                await _twilioClient.SendMessage(_smsAuthenticationOptions.TwilioSmsCredentials, phoneNumber, message).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _eventSource.Failure(ex);
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, "the twilio account is not properly configured");
            }

            if (!await _confirmationCodeStore.Add(confirmationCode).ConfigureAwait(false))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
            }

            _eventSource.GetConfirmationCode(confirmationCode.Value);
            return confirmationCode.Value;
        }

        private async Task<string> GetCode()
        {
            var random = new Random();
            var number = random.Next(100000, 999999);
            if (await _confirmationCodeStore.Get(number.ToString()).ConfigureAwait(false) != null)
            {
                return await GetCode().ConfigureAwait(false);
            }

            return number.ToString();
        }
    }
}
