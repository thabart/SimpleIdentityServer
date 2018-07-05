using System.Threading.Tasks;
using SimpleIdentityServer.Twilio.Client;

namespace SimpleIdentityServer.Host.Tests.Fakes
{
    internal sealed class FakeTwilioClient : ITwilioClient
    {
        public Task<bool> SendMessage(TwilioSmsCredentials credentials, string toPhoneNumber, string message)
        {
            return Task.FromResult(true);
        }
    }
}
