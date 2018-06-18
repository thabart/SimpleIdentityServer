#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.TwoFactorAuthentication.Twilio
{
    public class TwilioOptions
    {
        public string TwilioAccountSid { get; set; }
        public string TwilioAuthToken { get; set; }
        public string TwilioFromNumber { get; set; }
        public string TwilioMessage { get; set; }
    }

    public class DefaultTwilioSmsService : ITwoFactorAuthenticationService
    {
        private const string TwilioSmsEndpointFormat = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";
        private class TwilioSmsCredentials
        {
            public string AccountSid { get; set; } = string.Empty;
            public string AuthToken { get; set; } = string.Empty;
            public string FromNumber { get; set; }
        }

        private readonly IHttpClientFactory _clientFactory;
        private readonly TwilioOptions _options;

        public DefaultTwilioSmsService(TwilioOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _clientFactory = new HttpClientFactory();
        }

        public async Task SendAsync(string code, ResourceOwner user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.Claims == null)
            {
                throw new ArgumentNullException(nameof(user.Claims));
            }

            var phoneNumberClaim = user.Claims.FirstOrDefault(c => c.Type == "phone_number");
            if (phoneNumberClaim == null)
            {
                throw new ArgumentException("the phone number is missing");
            }
            
            await SendMessage(new TwilioSmsCredentials
            {
                AccountSid = _options.TwilioAccountSid,
                AuthToken = _options.TwilioAuthToken,
                FromNumber = _options.TwilioFromNumber,
            }, phoneNumberClaim.Value, string.Format(_options.TwilioMessage, code));
        }

        private async Task<bool> SendMessage(
            TwilioSmsCredentials credentials,
            string toPhoneNumber, 
            string message)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                throw new ArgumentException(nameof(toPhoneNumber));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(nameof(message));
            }

            var client = _clientFactory.GetHttpClient();
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
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        private string CreateBasicAuthenticationHeader(string username, string password)
        {
            var credentials = username + ":" + password;
            var encoded = System.Text.Encoding.UTF8.GetBytes(credentials);
            return Convert.ToBase64String(encoded);
        }
    }
}
