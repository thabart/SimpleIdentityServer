using SimpleIdentityServer.Twilio.Client;

namespace SimpleIdentityServer.Authenticate.SMS
{
    public class SmsAuthenticationOptions
    {
        public SmsAuthenticationOptions()
        {
            Message = "The confirmation code is {0}";
        }

        public TwilioSmsCredentials TwilioSmsCredentials { get; set; }
        public string Message { get; set; }
    }
}
