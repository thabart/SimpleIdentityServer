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
using SimpleIdentityServer.Twilio.Client;
using System;
using System.Linq;
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
        private readonly TwilioOptions _options;
        private readonly ITwilioClient _twilioClient;

        public DefaultTwilioSmsService(TwilioOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _twilioClient = new TwilioClient();
        }

        public string RequiredClaim { get { return Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber; } }

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

            var phoneNumberClaim = user.Claims.FirstOrDefault(c => c.Type == RequiredClaim);
            if (phoneNumberClaim == null)
            {
                throw new ArgumentException("the phone number is missing");
            }
            
            await _twilioClient.SendMessage(new TwilioSmsCredentials
            {
                AccountSid = _options.TwilioAccountSid,
                AuthToken = _options.TwilioAuthToken,
                FromNumber = _options.TwilioFromNumber,
            }, phoneNumberClaim.Value, string.Format(_options.TwilioMessage, code));
        }
    }
}
