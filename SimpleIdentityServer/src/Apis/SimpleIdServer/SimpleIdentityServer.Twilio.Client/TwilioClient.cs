using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Twilio.Client
{
    public interface ITwilioClient
    {
        Task<bool> SendMessage(TwilioSmsCredentials credentials, string toPhoneNumber, string message);
    }

    public class TwilioClient : ITwilioClient
    {
        private const string TwilioSmsEndpointFormat = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";

        public async Task<bool> SendMessage(TwilioSmsCredentials credentials, string toPhoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                throw new ArgumentException(nameof(toPhoneNumber));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(nameof(message));
            }

            var client = new HttpClient();
            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("To", toPhoneNumber));
            keyValues.Add(new KeyValuePair<string, string>("From", credentials.FromNumber));
            keyValues.Add(new KeyValuePair<string, string>("Body", message));
            var content = new FormUrlEncodedContent(keyValues);
            var postUrl = string.Format(CultureInfo.InvariantCulture, TwilioSmsEndpointFormat, credentials.AccountSid);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(postUrl)
            };
            httpRequest.Headers.Add("User-Agent", "twilio-csharp/5.13.4 (.NET Framework 4.5.1+)");
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Accept-Encoding", "utf-8");
            httpRequest.Headers.Add("Authorization", "Basic " + CreateBasicAuthenticationHeader(credentials.AccountSid, credentials.AuthToken));
            var response = await client.SendAsync(httpRequest).ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new TwilioException(json);
            }

            return true;
        }
        private string CreateBasicAuthenticationHeader(string username, string password)
        {
            var credentials = username + ":" + password;
            var encoded = System.Text.Encoding.UTF8.GetBytes(credentials);
            return Convert.ToBase64String(encoded);
        }
    }
}
