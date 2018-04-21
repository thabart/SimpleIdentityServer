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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.TwoFactorAuthentication.Email
{
    public class EmailServiceOptions
    {
        public string EmailFromName { get; set; }
        public string EmailFromAddress { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailSmtpHost { get; set; }
        public int EmailSmtpPort { get; set; }
        public bool EmailSmtpUseSsl { get; set; }
        public string EmailUserName { get; set; }
        public string EmailPassword { get; set; }
    }

    public class DefaultEmailService : ITwoFactorAuthenticationService
    {
        private readonly EmailServiceOptions _options;

        public DefaultEmailService(EmailServiceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            _options = options;
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
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.EmailFromName, _options.EmailFromAddress));
            message.To.Add(new MailboxAddress(displayName, emailClaim.Value));
            message.Subject = _options.EmailSubject;
            var bodyBuilder = new BodyBuilder()
            {
                HtmlBody = string.Format(_options.EmailBody, code)
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => {
                    return true;
                };
                await client.ConnectAsync(_options.EmailSmtpHost, _options.EmailSmtpPort, _options.EmailSmtpUseSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_options.EmailUserName, _options.EmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
