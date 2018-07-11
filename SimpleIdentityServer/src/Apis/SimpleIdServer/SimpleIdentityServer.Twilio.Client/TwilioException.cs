using System;

namespace SimpleIdentityServer.Twilio.Client
{
    public class TwilioException : Exception
    {
        public TwilioException(string message) : base(message) { }
    }
}
