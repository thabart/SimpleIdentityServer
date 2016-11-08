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

using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Configuration.Client.Setting;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.TwoFactors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.TwoFactors
{
    public class TwilioSmsService : ITwoFactorAuthenticationService
    {
        private const string TwilioSmsEndpointFormat = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";
        private const string TwilioAccountSid = "TwilioAccountSid";
        private const string TwilioAuthToken = "TwilioAuthToken";
        private const string TwilioFromNumber = "TwilioFromNumber";
        private const string TwilioMessage = "TwilioMessage";

        private List<string> _settingNames = new List<string>
        {
            TwilioAccountSid,
            TwilioAuthToken,
            TwilioFromNumber,
            TwilioMessage
        };

        private class TwilioSmsCredentials
        {
            public string AccountSid { get; set; } = string.Empty;
            public string AuthToken { get; set; } = string.Empty;
            public string FromNumber { get; set; }
        }

        private readonly IHttpClientFactory _clientFactory;

        private readonly ISettingClient _settingClient;

        private readonly string _configurationUrl;

        public TwilioSmsService(
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            string configurationUrl)
        {
            if (simpleIdServerConfigurationClientFactory == null)
            {
                throw new ArgumentNullException(nameof(simpleIdServerConfigurationClientFactory));
            }

            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            _settingClient = simpleIdServerConfigurationClientFactory.GetSettingClient();
            _configurationUrl = configurationUrl;
            _clientFactory = new HttpClientFactory();
        }

        public int Code
        {
            get
            {
                return (int)TwoFactorAuthentications.Sms;
            }
        }

        public async Task SendAsync(string code, ResourceOwner user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                throw new ArgumentException("the phone number is missing");
            }

            var settings = await _settingClient.GetSettingsByResolving(_configurationUrl);
            if (!_settingNames.All(k => settings.Any(s => s.Key == k)))
            {
                throw new InvalidOperationException("there are one or more missing settings");
            }

            var dic = settings.ToDictionary(s => s.Key);
            await SendMessage(new TwilioSmsCredentials
            {
             AccountSid = dic[TwilioAccountSid].Value,
             AuthToken = dic[TwilioAuthToken].Value,
             FromNumber = dic[TwilioFromNumber].Value,
            }, user.PhoneNumber, string.Format(dic[TwilioMessage].Value, code));
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
            client.DefaultRequestHeaders.Authorization = CreateBasicAuthenticationHeader(
                credentials.AccountSid, 
                credentials.AuthToken);

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("To", toPhoneNumber));
            keyValues.Add(new KeyValuePair<string, string>("From", credentials.FromNumber));
            keyValues.Add(new KeyValuePair<string, string>("Body", message));

            var content = new FormUrlEncodedContent(keyValues);
            
            var postUrl = string.Format(
                    CultureInfo.InvariantCulture,
                    TwilioSmsEndpointFormat,
                    credentials.AccountSid);

            var response = await client.PostAsync(
                postUrl, 
                content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        private AuthenticationHeaderValue CreateBasicAuthenticationHeader(string username, string password)
        {
            return new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(
                     string.Format("{0}:{1}", username, password)
                     )
                 )
            );
        }
    }
}
