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

using MailKit.Net.Smtp;
using MimeKit;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Configuration.Client.Setting;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Services
{
    public class DefaultEmailService : ITwoFactorAuthenticationService
    {
        private const string EmailFromName = "EmailFromName";
        private const string EmailFromAddress = "EmailFromAddress";
        private const string EmailSubject = "EmailSubject";
        private const string EmailBody = "EmailBody";
        private const string EmailSmtpHost = "EmailSmtpHost";
        private const string EmailSmtpPort = "EmailSmtpPort";
        private const string EmailSmtpUseSsl = "EmailSmtpUseSsl";
        private const string EmailUserName = "EmailUserName";
        private const string EmailPassword = "EmailPassword";

        private List<string> _settingNames = new List<string>
        {
            EmailFromName,
            EmailFromAddress,
            EmailSubject,
            EmailBody,
            EmailSmtpHost,
            EmailSmtpPort,
            EmailSmtpUseSsl,
            EmailUserName,
            EmailPassword
        };

        private readonly ISettingClient _settingClient;

        private readonly string _configurationUrl;

        public DefaultEmailService(
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
        }

        public int Code
        {
            get
            {
                return (int)TwoFactorAuthentications.Email;
            }
        }

        public async Task SendAsync(string code, ResourceOwner user)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.Claims == null)
            {
                throw new ArgumentNullException(nameof(user.Claims));
            }

            // 1. Try to fetch the email.
            var emailClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email);
            if (emailClaim == null)
            {
                throw new ArgumentException("the email is not present");
            }

            // 2. Try to fetch the display name.
            string displayName;
            var displayNameClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name);
            if (displayNameClaim == null)
            {
                displayName = user.Id;
            }
            else
            {
                displayName = displayNameClaim.Value;
            }
            var settings = await _settingClient.GetSettingsByResolving(_configurationUrl).ConfigureAwait(false);
            if (!_settingNames.All(k => settings.Any(s => s.Key == k)))
            {
                throw new InvalidOperationException("there are one or more missing settings");
            }

            var dic = settings.ToDictionary(s => s.Key);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(dic[EmailFromName].Value, dic[EmailFromAddress].Value));
            message.To.Add(new MailboxAddress(displayName, emailClaim.Value));
            message.Subject = dic[EmailSubject].Value;
            var bodyBuilder = new BodyBuilder()
            {
                HtmlBody = string.Format(dic[EmailBody].Value, code)
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => {
                    return true;
                };
                await client.ConnectAsync(dic[EmailSmtpHost].Value, int.Parse(dic[EmailSmtpPort].Value), bool.Parse(dic[EmailSmtpUseSsl].Value)).ConfigureAwait(false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(dic[EmailUserName].Value, dic[EmailPassword].Value).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
